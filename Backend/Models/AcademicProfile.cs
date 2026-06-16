using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class AcademicProfile
{
    public Guid UserId { get; set; }

    public string? Title { get; set; }

    public string? Specialization { get; set; }

    public virtual User User { get; set; } = null!;
}
