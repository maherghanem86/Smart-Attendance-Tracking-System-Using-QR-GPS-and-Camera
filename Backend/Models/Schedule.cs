using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class Schedule
{
    public Guid Id { get; set; }

    public Guid? SectionId { get; set; }

    public Guid? RoomId { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<AttendanceSession> AttendanceSessions { get; set; } = new List<AttendanceSession>();

    public virtual Room? Room { get; set; }

    public virtual Section? Section { get; set; }
}
