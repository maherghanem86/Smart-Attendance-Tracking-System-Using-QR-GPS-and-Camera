using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class Enrollment
{
    public Guid Id { get; set; }

    public Guid? StudentId { get; set; }

    public Guid? SectionId { get; set; }

    public DateOnly? EnrollmentDate { get; set; }

    public virtual Section? Section { get; set; }

    public virtual User? Student { get; set; }
}
