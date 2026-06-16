using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class AbsenceExcuse
{
    public Guid Id { get; set; }

    public Guid? StudentId { get; set; }

    public Guid? SessionId { get; set; }

    public string? ExcuseDetails { get; set; }

    public string? AttachmentPath { get; set; }

    public string? Status { get; set; }

    public virtual AttendanceSession? Session { get; set; }

    public virtual User? Student { get; set; }
}
