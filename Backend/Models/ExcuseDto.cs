namespace SmartAttendance.API.Models
{
    public class ExcuseDto
    {
        // 🌟 التعديل هنا: تحويل النوع من Guid إلى string
        public required string SessionId { get; set; }

        public required string Reason { get; set; }

        public IFormFile? Attachment { get; set; }
    }
}