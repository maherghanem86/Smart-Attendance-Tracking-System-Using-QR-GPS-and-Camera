using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual ICollection<AbsenceExcuse> AbsenceExcuses { get; set; } = new List<AbsenceExcuse>();

    public virtual AcademicProfile? AcademicProfile { get; set; }

    public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();

    public virtual ICollection<AuditTrail> AuditTrails { get; set; } = new List<AuditTrail>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<SecurityAlert> SecurityAlerts { get; set; } = new List<SecurityAlert>();

    public virtual StudentProfile? StudentProfile { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
