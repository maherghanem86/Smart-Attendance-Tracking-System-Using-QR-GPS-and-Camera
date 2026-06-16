using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // ==============================================================================
        // 1. تقرير الشعبة: يظهر الطلاب المسجلين ونسبة حضورهم (للأدمن والمدرس)
        // ==============================================================================
        [HttpGet("Section/{sectionId}")]
        public async Task<IActionResult> GetSectionReport(Guid sectionId)
        {
            var totalSessions = await _context.AttendanceSessions
                .Include(s => s.Schedule)
                .Where(s => s.Schedule != null && s.Schedule.SectionId == sectionId && s.IsActive == true)
                .CountAsync();

            if (totalSessions == 0) return Ok(new List<object>());

            var reportData = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(u => u!.StudentProfile)
                .Where(e => e.SectionId == sectionId)
                .Select(e => new
                {
                    StudentName = e.Student != null ? e.Student.Username : "غير معروف",
                    UniversityId = (e.Student != null && e.Student.StudentProfile != null) ? e.Student.StudentProfile.UniversityId : "---",
                    TotalSessions = totalSessions,
                    Attended = _context.AttendanceLogs
                        .Count(l => l.StudentId == e.StudentId &&
                                    l.Session != null &&
                                    l.Session.Schedule != null &&
                                    l.Session.Schedule.SectionId == sectionId &&
                                    l.Status == "Present")
                })
                .ToListAsync();

            var finalResult = reportData.Select(r => new
            {
                r.StudentName,
                r.UniversityId,
                r.TotalSessions,
                r.Attended,
                Percentage = r.TotalSessions > 0
                             ? Math.Round(((double)r.Attended / r.TotalSessions) * 100, 1) + "%"
                             : "0%"
            });

            return Ok(finalResult);
        }

        // ==============================================================================
        // 2. تقرير الأعذار الشامل للأدمن: يرى كل الأعذار والمدرس الموجهة له وحالتها
        // ==============================================================================
        [HttpGet("Admin/AllExcuses")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminExcusesReport()
        {
            var excuses = await _context.AbsenceExcuses
                .Include(e => e.Student)
                .Include(e => e.Session)
                    .ThenInclude(s => s!.Schedule)
                        .ThenInclude(sch => sch!.Section)
                            .ThenInclude(sec => sec!.Course)
                .Include(e => e.Session!.Schedule!.Section!.Instructor)
                .OrderByDescending(e => e.Session!.SessionDate)
                .Select(e => new {
                    Id = e.Id,
                    StudentName = e.Student != null ? e.Student.Username : "غير معروف",
                    CourseName = e.Session!.Schedule!.Section!.Course!.Name,
                    SectionNumber = e.Session.Schedule.SectionId,
                    InstructorName = e.Session.Schedule.Section.Instructor != null ? e.Session.Schedule.Section.Instructor.Username : "غير محدد",
                    Reason = e.ExcuseDetails,
                    Status = e.Status, // Approved, Rejected, Pending
                    DateSubmitted = e.Session.SessionDate
                })
                .ToListAsync();

            return Ok(excuses);
        }

        // ==============================================================================
        // 3. تقرير محاولات التلاعب (للأدمن)
        // ==============================================================================
        [HttpGet("Admin/SecurityAlerts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSecurityAlerts()
        {
            var alerts = await _context.SecurityAlerts
                .Include(a => a.User)
                .OrderByDescending(a => a.DetectedAt)
                .Select(a => new {
                    StudentName = a.User != null ? a.User.Username : "Unknown",
                    Details = a.AlertDescription,
                    Severity = a.Severity,
                    Time = a.DetectedAt
                })
                .ToListAsync();

            return Ok(alerts);
        }

        [HttpGet("Instructor/FraudAlerts")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorFraudAlerts()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // 🌟 إصلاح التحذير: التحقق من القيمة قبل تحويلها إلى Guid
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("لم يتم العثور على معرف المستخدم في التوكن.");
            }

            var userId = Guid.Parse(userIdString);

            // جلب التنبيهات الخاصة بالطلاب المسجلين في شعب هذا المدرس فقط
            var alerts = await _context.SecurityAlerts
                .Include(a => a.User)
                .Where(a => _context.Enrollments
                    // 🌟 إصلاح التحذير: استخدام عامل Null-forgiving (!) للكائنات المرتبطة
                    .Any(e => e.StudentId == a.UserId && e.Section!.InstructorId == userId))
                .OrderByDescending(a => a.DetectedAt)
                .Select(a => new {
                    StudentName = a.User!.Username, // 🌟 استخدام ! للمستخدم
                    AlertDescription = a.AlertDescription,
                    Severity = a.Severity,
                    DetectedAt = a.DetectedAt
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // ==============================================================================
        // 4. تقرير التتبع الشامل للطلاب (للأدمن)
        // ==============================================================================
        [HttpGet("Admin/GlobalStudentTracking")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGlobalStudentTracking()
        {
            var studentsTracking = await _context.Users
                .Include(u => u.Roles) // 🌟 جلب الأدوار
                .Include(u => u.StudentProfile)
                    .ThenInclude(sp => sp!.Major) // جلب القسم/التخصص
                .Include(u => u.Enrollments)
                    .ThenInclude(e => e.Section!)
                        .ThenInclude(s => s.Course) // جلب المادة
                .Include(u => u.Enrollments)
                    .ThenInclude(e => e.Section!)
                        .ThenInclude(s => s.Instructor) // جلب اسم المدرس
                                                        // 🌟 التعديل الجوهري: جلب أي مستخدم لديه دور "طالب"
                .Where(u => u.Roles.Any(r => r.RoleName == "Student"))
                .Select(u => new
                {
                    studentName = u.Username,

                    // 🌟 معالجة حالة الطالب الجديد الذي لم يختر قسمه بعد
                    department = u.StudentProfile != null && u.StudentProfile.Major != null
                                 ? u.StudentProfile.Major.Name
                                 : "طالب جديد (لم يكمل ملفه)",

                    enrollments = u.Enrollments.Select(e => new
                    {
                        courseName = e.Section!.Course != null ? e.Section.Course.Name : "غير معروف",
                        instructorName = e.Section!.Instructor != null ? e.Section.Instructor.Username : "غير محدد",
                        sectionId = e.SectionId.ToString().Substring(0, 4).ToUpper()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(studentsTracking);
        }

        // ==============================================================================
        // 5. تقرير التتبع الشامل للمدرسين (للأدمن) [الإضافة الجديدة]
        // ==============================================================================
        [HttpGet("Admin/GlobalInstructorTracking")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGlobalInstructorTracking()
        {
            var instructorsTracking = await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.RoleName == "Instructor"))
                .Select(u => new
                {
                    instructorName = u.Username,
                    email = u.Email,
                    // جلب الشعب التي تم إسنادها لهذا المدرس مع المادة الخاصة بها
                    assignedSections = _context.Sections
                        .Include(s => s.Course)
                        .Where(s => s.InstructorId == u.Id)
                        .Select(s => new
                        {
                            courseName = s.Course != null ? s.Course.Name : "مادة غير معروفة",
                            sectionId = s.Id.ToString().Substring(0, 4).ToUpper()
                        }).ToList()
                })
                .ToListAsync();

            return Ok(instructorsTracking);
        }
    }
}