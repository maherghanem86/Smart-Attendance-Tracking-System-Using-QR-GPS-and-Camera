using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartAttendance.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            // 1. جلب المستخدم مع دوره
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
            }

            if (user.IsActive == false)
            {
                return Unauthorized(new { message = "هذا الحساب معطل." });
            }

            var userRole = user.Roles.FirstOrDefault()?.RoleName ?? "Student";

            // =======================================================================
            // 🔥 الإصلاح هنا: توحيد مفتاح التشفير يدوياً لضمان التطابق 🔥
            // نستخدم نفس المفتاح الموجود في appsettings.json بالضبط
            // =======================================================================
            var jwtKeyString = "ThisIsMySecretKeyForMastersProject2026_MustBeLong";
            var key = Encoding.UTF8.GetBytes(jwtKeyString);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, userRole)
                }),
                Expires = DateTime.UtcNow.AddDays(7),

                // توحيد المصدر والجمهور أيضاً لضمان القبول
                Issuer = "SmartAttendanceAPI",
                Audience = "SmartAttendanceUser",

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                userId = user.Id,
                username = user.Username,
                role = userRole
            });
        }
    }
}