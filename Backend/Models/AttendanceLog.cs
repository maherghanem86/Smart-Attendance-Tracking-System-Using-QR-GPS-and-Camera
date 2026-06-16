using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace SmartAttendance.API.Models;

public partial class AttendanceLog
{
    public Guid Id { get; set; }

    public Guid? StudentId { get; set; }

    public Guid? SessionId { get; set; }

    public DateTimeOffset? CheckInTime { get; set; }

    public Geometry? CapturedLocation { get; set; }

    public string? VerificationMetadata { get; set; }

    public string? Status { get; set; }

    public virtual AttendanceSession? Session { get; set; }

    public virtual User? Student { get; set; }
}
