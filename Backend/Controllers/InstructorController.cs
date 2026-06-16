using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;
using System.Security.Claims;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Instructor,Admin")] // السماح للمدرس والأدمن فقط
    public class InstructorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InstructorController(AppDbContext context)
        {
            _context = context;
        }

        // ==============================================================================
        // 1. جلب المواد الخاصة بالمدرس (الموجود مسبقاً)
        // ==============================================================================
        [HttpGet("MyCourses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var instructorId = Guid.Parse(userIdString);

            var courses = await _context.Sections
                .Include(s => s.Course)
                .Where(s => s.InstructorId == instructorId)
                .Select(s => new
                {
                    SectionId = s.Id,
                    CourseName = s.Course != null ? s.Course.Name : "Unknown",
                    CourseCode = s.Course != null ? s.Course.CourseCode : "---",
                    Semester = s.Semester,
                    Year = s.Year
                })
                .ToListAsync();

            return Ok(courses);
        }

        // ==============================================================================
        // 2. الحضور المباشر للجلسة (الموجود مسبقاً)
        // ==============================================================================
        [HttpGet("Session/{sessionId}/Live")]
        public async Task<IActionResult> GetLiveAttendance(Guid sessionId)
        {
            var logs = await _context.AttendanceLogs
                .Include(l => l.Student)
                    .ThenInclude(u => u!.StudentProfile)
                .Where(l => l.SessionId == sessionId && l.Status == "Present")
                .OrderByDescending(l => l.CheckInTime)
                .Select(l => new
                {
                    StudentName = l.Student != null ? l.Student.Username : "Unknown",
                    UniversityId = (l.Student != null && l.Student.StudentProfile != null)
                                   ? l.Student.StudentProfile.UniversityId : "---",
                    CheckInTime = l.CheckInTime.HasValue
                                  ? l.CheckInTime.Value.ToString("hh:mm tt") : "--:--",
                    Status = l.Status,
                    ProfileImage = (l.Student != null && l.Student.StudentProfile != null)
                                   ? l.Student.StudentProfile.ProfilePicturePath : null,
                    VerificationScore = "100%"
                })
                .ToListAsync();

            return Ok(logs);
        }

        // ==============================================================================
        // 3. جلب محاولات التلاعب الجغرافي لطلاب المدرس [جديــــد]
        // ==============================================================================
        [HttpGet("MyFraudAlerts")]
        public async Task<IActionResult> GetMyFraudAlerts()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var instructorId = Guid.Parse(userIdString);

            // جلب التنبيهات الأمنية للطلاب الذين هم أصلاً مسجلون في أي مادة يدرسها هذا المدرس
            var alerts = await _context.SecurityAlerts
                .Include(a => a.User)
                .Where(a => _context.Enrollments.Any(e =>
                    e.StudentId == a.UserId &&
                    e.Section != null &&
                    e.Section.InstructorId == instructorId))
                .OrderByDescending(a => a.DetectedAt)
                .Select(a => new {
                    Id = a.Id,
                    StudentName = a.User != null ? a.User.Username : "Unknown",
                    AlertDescription = a.AlertDescription,
                    Severity = a.Severity,
                    DetectedAt = a.DetectedAt
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // ==============================================================================
        // 4. جلب الأعذار الطبية المعلقة لطلاب المدرس [جديــــد]
        // ==============================================================================
        [HttpGet("MyPendingExcuses")]
        public async Task<IActionResult> GetMyPendingExcuses()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var instructorId = Guid.Parse(userIdString);

            var excuses = await _context.AbsenceExcuses
                .Include(e => e.Student)
                .Include(e => e.Session)
                    .ThenInclude(s => s!.Schedule)
                        .ThenInclude(sch => sch!.Section)
                            .ThenInclude(sec => sec!.Course)
                .Where(e => e.Status == "Pending" &&
                            e.Session != null &&
                            e.Session.Schedule != null &&
                            e.Session.Schedule.Section != null &&
                            e.Session.Schedule.Section.InstructorId == instructorId)
                .Select(e => new {
                    Id = e.Id,
                    StudentName = e.Student != null ? e.Student.Username : "Unknown",
                    CourseName = e.Session!.Schedule!.Section!.Course!.Name,
                    ExcuseDetails = e.ExcuseDetails,
                    AttachmentPath = e.AttachmentPath,
                    Status = e.Status,
                    SessionDate = e.Session.SessionDate
                })
                .ToListAsync();

            return Ok(excuses);
        }
    }
}