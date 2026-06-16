using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SectionsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. جلب كل الشعب مع بيانات المادة والمدرس
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Section>>> GetSections()
        {
            return await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                .Include(s => s.Schedules) // مهم جداً لعرض المواعيد في واجهة الأدمن
                .ToListAsync();
        }

        // 2. دالة جلب شعبة واحدة (ضرورية لنجاح عملية الإضافة)
        [HttpGet("{id}")]
        public async Task<ActionResult<Section>> GetSection(Guid id)
        {
            var section = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (section == null) return NotFound();
            return section;
        }

        // 3. إضافة شعبة جديدة
        [HttpPost]
        public async Task<ActionResult<Section>> PostSection(Section section)
        {
            if (section.Id == Guid.Empty) section.Id = Guid.NewGuid();

            _context.Sections.Add(section);
            await _context.SaveChangesAsync();

            // إعادة جلب البيانات مع الـ Includes لكي يراها الـ Flutter فوراً
            var result = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(s => s.Id == section.Id);

            return CreatedAtAction(nameof(GetSection), new { id = section.Id }, result);
        }

        // 4. تعديل شعبة موجودة (هذه كانت مفقودة لديك)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSection(Guid id, Section section)
        {
            if (id != section.Id) return BadRequest("ID mismatch");

            _context.Entry(section).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SectionExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        private bool SectionExists(Guid id)
        {
            return _context.Sections.Any(e => e.Id == id);
        }
    }
}