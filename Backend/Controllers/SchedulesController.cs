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
    public class SchedulesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SchedulesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Schedules
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
        {
            // نستخدم Include لجلب بيانات القاعة والشعبة المرتبطة بالجدول
            // هذا سيجعل الرد يحتوي على (اسم القاعة، رقم الغرفة) بدلاً من المعرفات فقط
            return await _context.Schedules
                .Include(s => s.Room)      // تضمين بيانات القاعة
                .Include(s => s.Section)   // تضمين بيانات الشعبة
                .ToListAsync();
        }

        // GET: api/Schedules/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Schedule>> GetSchedule(Guid id)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }

            return schedule;
        }

        // PUT: api/Schedules/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchedule(Guid id, Schedule schedule)
        {
            if (id != schedule.Id) return BadRequest();

            // التحقق من التعارض (منع حجز قاعة مشغولة) عند التعديل
            var conflict = await _context.Schedules.AnyAsync(s =>
                s.Id != id && // لا نتحقق من نفس الموعد الذي نعدله
                s.RoomId == schedule.RoomId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                ((schedule.StartTime >= s.StartTime && schedule.StartTime < s.EndTime) ||
                 (schedule.EndTime > s.StartTime && schedule.EndTime <= s.EndTime))
            );

            if (conflict)
            {
                return BadRequest(new { message = "يوجد تعارض! القاعة محجوزة في هذا التوقيت." });
            }

            _context.Entry(schedule).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Schedules
        [HttpPost]
        public async Task<ActionResult<Schedule>> PostSchedule(Schedule schedule)
        {
            // 1. التحقق: هل القاعة محجوزة في نفس اليوم ونفس التوقيت؟
            // (Overlap Logic)
            var conflict = await _context.Schedules.AnyAsync(s =>
                s.RoomId == schedule.RoomId &&
                s.DayOfWeek == schedule.DayOfWeek &&
                ((schedule.StartTime >= s.StartTime && schedule.StartTime < s.EndTime) || // بداية المحاضرة الجديدة تقع داخل محاضرة قديمة
                 (schedule.EndTime > s.StartTime && schedule.EndTime <= s.EndTime))       // نهاية المحاضرة الجديدة تقع داخل محاضرة قديمة
            );

            if (conflict)
            {
                return BadRequest(new { message = "يوجد تعارض! القاعة محجوزة في هذا التوقيت." });
            }

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSchedule", new { id = schedule.Id }, schedule);
        }

        // DELETE: api/Schedules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(Guid id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ScheduleExists(Guid id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}
