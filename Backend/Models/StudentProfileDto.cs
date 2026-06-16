namespace SmartAttendance.API.Models
{
    public class StudentProfileDto
    {
        public required string UniversityId { get; set; } // الرقم الجامعي
        public Guid? MajorId { get; set; } // التخصص
        public int CurrentSemester { get; set; }
        public IFormFile? ProfileImage { get; set; } // ملف الصورة القادم من الموبايل
    }
}