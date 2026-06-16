using System;
using System.Collections.Generic;

namespace SmartAttendance.API.Models;

public partial class SystemSetting
{
    public string Key { get; set; } = null!;

    public string? Value { get; set; }

    public string? Description { get; set; }
}
