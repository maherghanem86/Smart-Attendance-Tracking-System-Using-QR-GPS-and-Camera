using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public bool? IsRead { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
