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
    // =======================================================
    // 🌟 DTO مرن جداً يتقبل البيانات بأسماء مختلفة من الفلوتر
    // =======================================================
    public class UserCreateDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Password { get; set; } // في حال كان الفلوتر يرسلها باسم password
        public string? Role { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // التعديل هنا: إضافة Include لجلب الأدوار (Roles) الخاصة بكل مستخدم
            return await _context.Users.Include(u => u.Roles).ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // ============================================================
        // التعديل الجوهري: دالة الإضافة أصبحت تستقبل الـ DTO لربط الدور وتقبل النواقص
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] UserCreateDto dto)
        {
            // 1. إنشاء الكائن الجديد للمستخدم (مع قيم افتراضية آمنة في حال نقص البيانات)
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username ?? "مستخدم جديد",
                Email = dto.Email ?? "no-email@system.com",
                // سحب كلمة المرور سواء أرسلها الفلوتر باسم PasswordHash أو Password
                PasswordHash = !string.IsNullOrEmpty(dto.PasswordHash) ? dto.PasswordHash : (!string.IsNullOrEmpty(dto.Password) ? dto.Password : "123456"),
                IsActive = true,
                CreatedAt = DateTimeOffset.Now
            };

            // 2. معالجة ذكية للكلمات القادمة من الفلوتر لضمان ربط الدور (Role)
            string roleName = string.IsNullOrEmpty(dto.Role) ? "Student" : dto.Role;

            // توحيد المصطلحات إذا كانت الواجهة ترسل بالعربي
            if (roleName == "طالب" || roleName.ToLower() == "student") roleName = "Student";
            else if (roleName == "مدرس" || roleName == "دكتور" || roleName.ToLower() == "instructor") roleName = "Instructor";
            else if (roleName == "مدير" || roleName == "أدمن" || roleName.ToLower() == "admin") roleName = "Admin";

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role != null)
            {
                user.Roles.Add(role);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 3. إعادة جلب المستخدم مع دوره
            var createdUser = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == user.Id);
            return Ok(createdUser);
        }


        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}