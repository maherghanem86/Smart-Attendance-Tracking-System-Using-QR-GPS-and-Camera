using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class Course
{
    public Guid Id { get; set; }

    public Guid? DeptId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? Credits { get; set; }

    public virtual Department? Dept { get; set; }

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
