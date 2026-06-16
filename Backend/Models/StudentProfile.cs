using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class StudentProfile
{
    public Guid UserId { get; set; }

    public string UniversityId { get; set; } = null!;

    public Guid? MajorId { get; set; }

    public int? CurrentSemester { get; set; }

    public string? FaceEncoding { get; set; }

    public string? ProfilePicturePath { get; set; }

    public virtual Department? Major { get; set; }

    public virtual User User { get; set; } = null!;
}
