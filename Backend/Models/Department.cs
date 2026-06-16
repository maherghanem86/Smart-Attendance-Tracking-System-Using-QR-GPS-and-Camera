using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class Department
{
    public Guid Id { get; set; }

    public Guid? FacultyId { get; set; }

    public string Name { get; set; } = null!;

    public string? DeptHead { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Faculty? Faculty { get; set; }

    public virtual ICollection<StudentProfile> StudentProfiles { get; set; } = new List<StudentProfile>();
}
