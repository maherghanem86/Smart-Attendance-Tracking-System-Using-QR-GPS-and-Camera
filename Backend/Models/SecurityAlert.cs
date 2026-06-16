using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class SecurityAlert
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? AlertDescription { get; set; }

    public string? Severity { get; set; }

    public DateTimeOffset? DetectedAt { get; set; }

    public virtual User? User { get; set; }
}
