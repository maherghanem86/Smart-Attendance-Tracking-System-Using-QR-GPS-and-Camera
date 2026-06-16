using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CampusesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Campus>>> GetCampuses()
        {
            return await _context.Campuses.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Campus>> PostCampus(Campus campus)
        {
            _context.Campuses.Add(campus);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCampuses", new { id = campus.Id }, campus);
        }
    }
}