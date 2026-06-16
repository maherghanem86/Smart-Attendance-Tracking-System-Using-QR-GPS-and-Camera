using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // حماية الكنترولر: يتطلب تسجيل دخول
    public class EnrollmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrollmentsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Enrollments
        [HttpPost]
        public async Task<IActionResult> EnrollStudent(EnrollmentDto dto)
        {
            var studentExists = await _context.Users.AnyAsync(u => u.Id == dto.StudentId);
            var sectionExists = await _context.Sections.AnyAsync(s => s.Id == dto.SectionId);

            if (!studentExists || !sectionExists)
            {
                return BadRequest(new { message = "بيانات الطالب أو الشعبة غير صحيحة." });
            }

            var exists = await _context.Enrollments.AnyAsync(e =>
                e.StudentId == dto.StudentId && e.SectionId == dto.SectionId);

            if (exists)
            {
                return BadRequest(new { message = "الطالب مسجل بالفعل في هذه الشعبة." });
            }

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = dto.StudentId,
                SectionId = dto.SectionId,
                EnrollmentDate = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تسجيل الطالب في المقرر بنجاح." });
        }

        // GET: api/Enrollments/Student/{studentId}
        [HttpGet("Student/{studentId}")]
        public async Task<IActionResult> GetStudentEnrollments(Guid studentId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Section)
                    .ThenInclude(s => s.Course)
                .Include(e => e.Section)
                    .ThenInclude(s => s.Instructor)
                .Where(e => e.StudentId == studentId)
                .Select(e => new
                {
                    SectionId = e.SectionId,
                    CourseName = e.Section != null && e.Section.Course != null ? e.Section.Course.Name : "N/A",
                    CourseCode = e.Section != null && e.Section.Course != null ? e.Section.Course.CourseCode : "N/A",
                    Instructor = e.Section != null && e.Section.Instructor != null ? e.Section.Instructor.Username : "N/A",

                    // حساب الجلسات المفتوحة لهذه الشعبة
                    TotalSessions = _context.AttendanceSessions
                        .Count(s => s.Schedule != null && s.Schedule.SectionId == e.SectionId && s.IsActive == true),

                    // حساب مرات حضور هذا الطالب بالذات
                    Attended = _context.AttendanceLogs
                        .Count(l => l.StudentId == studentId &&
                                    l.Session != null &&
                                    l.Session.Schedule != null &&
                                    l.Session.Schedule.SectionId == e.SectionId &&
                                    l.Status == "Present")
                })
                .ToListAsync();

            // حساب النسب المئوية للحضور
            var finalResult = enrollments.Select(r => new
            {
                r.SectionId,
                r.CourseName,
                r.CourseCode,
                r.Instructor,
                r.TotalSessions,
                r.Attended,
                Percentage = r.TotalSessions > 0
                             ? Math.Round(((double)r.Attended / r.TotalSessions) * 100, 1) + "%"
                             : "0%"
            });

            return Ok(finalResult);
        }
    }
}