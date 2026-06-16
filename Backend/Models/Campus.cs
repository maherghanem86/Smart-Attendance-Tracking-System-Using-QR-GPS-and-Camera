using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace SmartAttendance.API.Models;

public partial class Campus
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? City { get; set; }

    public string? Address { get; set; }

    public Geometry? Boundary { get; set; }

    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
}
