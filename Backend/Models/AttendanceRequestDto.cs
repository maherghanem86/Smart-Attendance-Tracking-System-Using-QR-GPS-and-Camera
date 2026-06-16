using Microsoft.AspNetCore.Http; // ضروري لـ IFormFile

namespace SmartAttendance.API.Models
{
    public class AttendanceRequestDto
    {
        public Guid StudentId { get; set; }

        public required string ScannedQrCode { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        // الصورة (IFormFile) ضرورية للتحقق البصري (Tri-Factor)
        public IFormFile? SelfieImage { get; set; }
    }
}