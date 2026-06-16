using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAttendance.API.Models;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // حماية الكنترولر: يتطلب توكن JWT للوصول
    public class AttendanceLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceLogsController(AppDbContext context)
        {
            _context = context;
        }

        // =================================================================================
        // دالة مساعدة لحساب المسافة الجغرافية بدقة (Haversine Formula)
        // تحسب المسافة بين نقطتين بالأمتار بناءً على انحناء الأرض
        // =================================================================================
        private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // نصف قطر الأرض بالأمتار
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // النتيجة بالأمتار
        }

        // POST: api/AttendanceLogs/CheckIn
        // نقطة النهاية الرئيسية لتسجيل الحضور
        [HttpPost("CheckIn")]
        public async Task<IActionResult> CheckIn([FromForm] AttendanceRequestDto request)
        {
            // =================================================================================
            // 1. جلب بيانات الجلسة والتحقق من صلاحية كود QR
            // =================================================================================
            var session = await _context.AttendanceSessions
                .Include(s => s.Schedule)
                    .ThenInclude(sch => sch!.Room) // للوصول لموقع القاعة
                .Include(s => s.Schedule)
                    .ThenInclude(sch => sch!.Section) // للوصول لبيانات الشعبة والمادة
                .FirstOrDefaultAsync(s => s.DynamicQrcode == request.ScannedQrCode && s.IsActive == true);

            if (session == null)
            {
                return BadRequest(new { message = "كود QR غير صحيح أو أن الجلسة منتهية الصلاحية." });
            }

            if (session.Schedule == null) return BadRequest(new { message = "بيانات الجدول الدراسي مفقودة." });
            if (session.Schedule.Section == null) return BadRequest(new { message = "بيانات الشعبة الدراسية مفقودة." });

            // =================================================================================
            // 2. التحقق من أن الطالب مسجل في المقرر (Enrollment Check)
            // =================================================================================
            var isEnrolled = await _context.Enrollments.AnyAsync(e =>
                e.StudentId == request.StudentId &&
                e.SectionId == session.Schedule.SectionId);

            if (!isEnrolled)
            {
                return BadRequest(new { message = "عذراً، أنت غير مسجل في هذا المقرر الدراسي، لا يمكنك تسجيل الحضور." });
            }

            // =================================================================================
            // 3. التحقق من التكرار (Duplicate Check)
            // =================================================================================
            var alreadyCheckedIn = await _context.AttendanceLogs
                .AnyAsync(l => l.StudentId == request.StudentId && l.SessionId == session.Id && l.Status == "Present");

            if (alreadyCheckedIn)
            {
                return BadRequest(new { message = "لقد قمت بتسجيل الحضور مسبقاً لهذه المحاضرة." });
            }

            // =================================================================================
            // 4. التحقق البصري (إلزامية صورة السيلفي) - [الإضافة الجديدة]
            // =================================================================================
            if (request.SelfieImage == null || request.SelfieImage.Length == 0)
            {
                return BadRequest(new { message = "صورة التحقق (السيلفي) مطلوبة لتسجيل الحضور." });
            }

            string? selfiePath = null;
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "attendance_selfies");

            // إنشاء المجلد إذا لم يكن موجوداً
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueName = Guid.NewGuid().ToString() + "_" + request.SelfieImage.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.SelfieImage.CopyToAsync(stream);
            }

            selfiePath = $"/uploads/attendance_selfies/{uniqueName}";

            // =================================================================================
            // 5. منطق التحقق الجغرافي (Geofencing Core Logic)
            // =================================================================================
            if (session.Schedule.Room == null || session.Schedule.Room.GeofenceCenter == null)
            {
                return BadRequest(new { message = "بيانات موقع القاعة غير محددة في النظام." });
            }

            var roomLat = session.Schedule.Room.GeofenceCenter.Coordinate.Y;
            var roomLon = session.Schedule.Room.GeofenceCenter.Coordinate.X;

            double distanceInMeters = CalculateDistanceInMeters(
                request.Latitude,
                request.Longitude,
                roomLat,
                roomLon
            );

            var allowedRadius = session.Schedule.Room.GeofenceRadius ?? 50;
            bool isWithinRange = distanceInMeters <= allowedRadius;

            var capturedPoint = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

            // =================================================================================
            // 6. التحقق النهائي من المسافة قبل الحفظ
            // =================================================================================
            if (!isWithinRange)
            {
                // اختيارياً: يمكنك تسجيل محاولة التلاعب في جدول الأمان قبل الرفض
                var alert = new SecurityAlert
                {
                    Id = Guid.NewGuid(),
                    UserId = request.StudentId,
                    AlertDescription = $"محاولة احتيال جغرافية: الطالب حاول تسجيل الحضور من مسافة {distanceInMeters:F2} متر.",
                    Severity = "High",
                    DetectedAt = DateTime.Now
                };
                _context.SecurityAlerts.Add(alert);
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    success = false,
                    message = $"فشل التحقق الجغرافي! أنت تبعد عن القاعة مسافة {distanceInMeters:F2} متر.",
                    distance = distanceInMeters
                });
            }

            // =================================================================================
            // 7. الحفظ في قاعدة البيانات فقط في حال النجاح (isWithinRange == true)
            // =================================================================================
            var log = new AttendanceLog
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                SessionId = session.Id,
                CheckInTime = DateTime.Now,
                CapturedLocation = capturedPoint,
                Status = "Present", // الحالة الآن دائماً حاضر لأنه تجاوز فحص المسافة
                VerificationMetadata = "{ \"DistanceMeters\": " + Math.Round(distanceInMeters, 2) + ", \"SelfieUrl\": \"" + selfiePath + "\" }"
            };

            _context.AttendanceLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "تم تسجيل الحضور بنجاح (التحقق الثلاثي مكتمل)!" });
        }
    

    [HttpPost("ManualCheckIn")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> ManualCheckIn([FromBody] ManualAttendanceDto dto)
        {
            // البحث عن الطالب بواسطة البريد الإلكتروني
            var student = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.StudentEmail);
            if (student == null) return NotFound(new { message = "الطالب غير موجود" });

            var log = new AttendanceLog
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                SessionId = dto.SessionId,
                CheckInTime = DateTime.Now,
                Status = "Present", // تحضير يدوي مباشر
                VerificationMetadata = "{ \"Method\": \"Manual\", \"By\": \"Instructor\" }"
            };

            _context.AttendanceLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "تم التحضير اليدوي بنجاح" });
        }

// ستحتاج لتعريف DTO بسيط لهذه العملية
public class ManualAttendanceDto
    {
        public required string StudentEmail { get; set; }
        public Guid SessionId { get; set; }
    }
}
}