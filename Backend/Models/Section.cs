using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class Section
{
    public Guid Id { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? InstructorId { get; set; }
    public string? Semester { get; set; }
    public int? Year { get; set; }

    // جعل الكائنات المرتبطة Nullable مهم جداً لنجاح الـ POST من Flutter
    public virtual Course? Course { get; set; }
    public virtual User? Instructor { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}