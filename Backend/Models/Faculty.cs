using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class Faculty
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? DeanName { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
