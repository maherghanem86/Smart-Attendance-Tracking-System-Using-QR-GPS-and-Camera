using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace SmartAttendance.API.Models;

public partial class Room
{
    public Guid Id { get; set; }

    public Guid? BuildingId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public int? Capacity { get; set; }

    public string? RoomType { get; set; }

    public Geometry? GeofenceCenter { get; set; }

    public double? GeofenceRadius { get; set; }

    public virtual Building? Building { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
