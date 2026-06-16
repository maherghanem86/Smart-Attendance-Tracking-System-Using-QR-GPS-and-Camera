using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceSessionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceSessionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AttendanceSessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceSession>>> GetAttendanceSessions()
        {
            return await _context.AttendanceSessions.ToListAsync();
        }

        // GET: api/AttendanceSessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AttendanceSession>> GetAttendanceSession(Guid id)
        {
            var attendanceSession = await _context.AttendanceSessions.FindAsync(id);

            if (attendanceSession == null)
            {
                return NotFound();
            }

            return attendanceSession;
        }

        // POST: api/AttendanceSessions
        [HttpPost]
        public async Task<ActionResult<AttendanceSession>> PostAttendanceSession(AttendanceSession session)
        {
            var inputId = session.ScheduleId;

            if (inputId == null)
            {
                return BadRequest(new { message = "يجب إرسال معرف الشعبة أو الجدول." });
            }

            // 1. معالجة ربط الجدول (نفس منطقك السابق الصحيح)
            var sectionExists = await _context.Sections.FindAsync(inputId);

            if (sectionExists != null)
            {
                var defaultRoom = await _context.Rooms.FirstOrDefaultAsync();
                var newSchedule = new Schedule
                {
                    Id = Guid.NewGuid(),
                    SectionId = inputId,
                    RoomId = defaultRoom?.Id,
                    DayOfWeek = (int)DateTime.Now.DayOfWeek,
                    StartTime = TimeOnly.FromDateTime(DateTime.Now),
                    EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(2))
                };

                _context.Schedules.Add(newSchedule);
                await _context.SaveChangesAsync();
                session.ScheduleId = newSchedule.Id;
            }
            else
            {
                var scheduleExists = await _context.Schedules.AnyAsync(s => s.Id == inputId);
                if (!scheduleExists)
                {
                    return BadRequest(new { message = "خطأ: المعرف المرسل غير صحيح." });
                }
            }

            // =========================================================
            // 🌟 التعديل الجوهري: توحيد المعرفات لحل مشاكل الربط 🌟
            // =========================================================

            // 1. إنشاء معرف فريد واحد للجلسة
            Guid sessionId = Guid.NewGuid();
            session.Id = sessionId;

            // 2. استخدام نفس المعرف داخل الـ QR Code مع الطابع الزمني
            // ملاحظة: الـ Flutter سيقوم الآن بمسح هذا النص وإرساله بالكامل
            session.DynamicQrcode = sessionId.ToString() + "_" + DateTime.Now.Ticks;

            session.SessionDate = DateOnly.FromDateTime(DateTime.Now);
            session.IsActive = true;
            session.OtpBackup = new Random().Next(1000, 9999).ToString();

            _context.AttendanceSessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAttendanceSession", new { id = session.Id }, session);
        }

        // DELETE: api/AttendanceSessions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendanceSession(Guid id)
        {
            var attendanceSession = await _context.AttendanceSessions.FindAsync(id);
            if (attendanceSession == null) return NotFound();

            _context.AttendanceSessions.Remove(attendanceSession);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/AttendanceSessions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAttendanceSession(Guid id, AttendanceSession attendanceSession)
        {
            if (id != attendanceSession.Id) return BadRequest();
            _context.Entry(attendanceSession).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceSessionExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        private bool AttendanceSessionExists(Guid id)
        {
            return _context.AttendanceSessions.Any(e => e.Id == id);
        }
    }
}