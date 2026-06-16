using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class AttendanceSession
{
    public Guid Id { get; set; }

    public Guid? ScheduleId { get; set; }

    public DateOnly? SessionDate { get; set; }

    public string? DynamicQrcode { get; set; }

    public bool? IsActive { get; set; }

    public string? OtpBackup { get; set; }

    public virtual ICollection<AbsenceExcuse> AbsenceExcuses { get; set; } = new List<AbsenceExcuse>();

    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();

    public virtual Schedule? Schedule { get; set; }
}
