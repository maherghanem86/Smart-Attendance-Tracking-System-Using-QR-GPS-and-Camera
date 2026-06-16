using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;
using System.Security.Claims;

namespace SmartAttendance.API.Controllers
{

    public class ReviewStatusDto
    {
        public required string Status { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExcusesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ExcusesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. تقديم عذر جديد (للطالب)
        [HttpPost]
        public async Task<IActionResult> SubmitExcuse([FromForm] ExcuseDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                var userId = Guid.Parse(userIdClaim.Value);

                // 🌟 الخطوة السحرية: البحث عن الجلسة الصحيحة
                // الطالب قد يرسل الـ Id الصافي أو نص الـ QR الطويل
                var session = await _context.AttendanceSessions
                    .FirstOrDefaultAsync(s => s.Id.ToString() == dto.SessionId.ToString() ||
                                             s.DynamicQrcode == dto.SessionId.ToString());

                if (session == null)
                {
                    return BadRequest(new { message = "خطأ: لم يتم العثور على هذه الجلسة في النظام." });
                }

                string? attachmentPath = null;
                if (dto.Attachment != null)
                {
                    // (كود حفظ الملف كما هو سابقاً...)
                    var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var uploadsFolder = Path.Combine(webRoot, "uploads", "excuses");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Attachment.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await dto.Attachment.CopyToAsync(stream); }
                    attachmentPath = $"/uploads/excuses/{uniqueName}";
                }

                var excuse = new AbsenceExcuse
                {
                    Id = Guid.NewGuid(),
                    StudentId = userId,
                    SessionId = session.Id, // 👈 نستخدم الـ Id الحقيقي الذي وجدناه في قاعدة البيانات
                    ExcuseDetails = dto.Reason,
                    AttachmentPath = attachmentPath,
                    Status = "Pending"
                };

                _context.AbsenceExcuses.Add(excuse);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تقديم العذر الطبي بنجاح." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 2. مراجعة العذر (قبول أو رفض) - للمدرس والأدمن
        [HttpPut("Review/{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> ReviewExcuse(Guid id, [FromBody] ReviewStatusDto dto)
        {
            string status = dto.Status;

            // جلب العذر مع بيانات الجلسة لمعرفة التاريخ
            var excuse = await _context.AbsenceExcuses
                .Include(e => e.Session)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (excuse == null) return NotFound();

            excuse.Status = status;

            if (status == "Approved")
            {
                var log = await _context.AttendanceLogs
                    .FirstOrDefaultAsync(l => l.StudentId == excuse.StudentId && l.SessionId == excuse.SessionId);

                if (log != null) { log.Status = "Excused"; }
                else
                {
                    _context.AttendanceLogs.Add(new AttendanceLog
                    {
                        Id = Guid.NewGuid(),
                        StudentId = excuse.StudentId,
                        SessionId = excuse.SessionId,
                        Status = "Excused",
                        CheckInTime = DateTime.Now
                    });
                }
            }

            // =============================================================
            // 🌟 الإضافة الجديدة: توليد الإشعار للطالب
            // =============================================================
            string statusArabic = status == "Approved" ? "قبول" : "رفض";
            string sessionDate = excuse.Session?.SessionDate?.ToString() ?? "الغير محدد تاريخها";

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = excuse.StudentId, // توجيه الإشعار للطالب صاحب العذر
                Title = $"تحديث حالة العذر الطبي",
                Message = $"تم {statusArabic} عذرك الطبي المقدم للمحاضرة بتاريخ {sessionDate}.",
                IsRead = false,
                CreatedAt = DateTimeOffset.Now
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
            return Ok(new { message = $"تم تحديث حالة العذر إلى: {status}" });
        }
    }
}