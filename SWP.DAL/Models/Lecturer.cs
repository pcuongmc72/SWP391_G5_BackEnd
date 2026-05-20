using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Lecturer
{
    public int LecturerId { get; set; }

    public int UserId { get; set; }

    public string LecturerCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Department { get; set; }

    public string? Title { get; set; }

    public virtual User User { get; set; } = null!;
}
