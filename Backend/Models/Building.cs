using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace SmartAttendance.API.Models;

public partial class Building
{
    public Guid Id { get; set; }

    public Guid? CampusId { get; set; }

    public string Name { get; set; } = null!;

    public Geometry? Location { get; set; }

    public virtual Campus? Campus { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
