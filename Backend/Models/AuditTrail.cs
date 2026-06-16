using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class AuditTrail
{
    public Guid Id { get; set; }

    public Guid? ActionBy { get; set; }

    public string? ActionType { get; set; }

    public string? TableName { get; set; }

    public string? OldData { get; set; }

    public string? NewData { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public virtual User? ActionByNavigation { get; set; }
}
