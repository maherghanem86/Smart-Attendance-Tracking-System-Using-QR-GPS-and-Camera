using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.API.Models;

namespace SmartAttendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // حماية الكنترولر (يجب أن يكون مسجلاً للدخول)
    public class StudentProfilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment; // للوصول لمجلدات السيرفر

        public StudentProfilesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: api/StudentProfiles/Upload
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadProfile([FromForm] StudentProfileDto dto)
        {
            // 1. معرفة المستخدم الحالي من التوكن (JWT)
            // التوكن يحتوي على الـ ID الخاص بالطالب، نستخرجه هنا
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            // 2. معالجة الصورة (رفع الملف)
            string imagePath = null;
            if (dto.ProfileImage != null)
            {
                // نحدد مكان الحفظ: wwwroot/uploads/profiles
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "profiles");

                // التأكد من وجود المجلد
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // إنشاء اسم فريد للملف (لتجنب تكرار الأسماء)
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ProfileImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // الحفظ الفعلي للملف
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfileImage.CopyToAsync(fileStream);
                }

                // المسار الذي سنخزنه في القاعدة (رابط)
                imagePath = $"/uploads/profiles/{uniqueFileName}";
            }

            // 3. الحفظ في قاعدة البيانات
            var profile = await _context.StudentProfiles.FindAsync(userId);

            if (profile == null)
            {
                // إنشاء بروفايل جديد
                profile = new StudentProfile
                {
                    UserId = userId,
                    UniversityId = dto.UniversityId,
                    MajorId = dto.MajorId,
                    CurrentSemester = dto.CurrentSemester,
                    ProfilePicturePath = imagePath
                };
                _context.StudentProfiles.Add(profile);
            }
            else
            {
                // تحديث بروفايل موجود
                profile.UniversityId = dto.UniversityId;
                profile.MajorId = dto.MajorId;
                profile.CurrentSemester = dto.CurrentSemester;
                if (imagePath != null) profile.ProfilePicturePath = imagePath; // تحديث الصورة فقط إذا تم رفع جديدة
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث الملف الشخصي ورفع الصورة بنجاح", path = imagePath });
        }

        // GET: api/StudentProfiles/MyProfile
        [HttpGet("MyProfile")]
        public async Task<ActionResult<StudentProfile>> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            var profile = await _context.StudentProfiles
                .Include(p => p.User)
                .Include(p => p.Major)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound(new { message = "لا يوجد ملف شخصي لهذا الطالب بعد" });

            return profile;
        }
    }
}