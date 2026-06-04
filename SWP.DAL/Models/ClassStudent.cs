using System;
using System.Collections.Generic;
namespace SWP.DAL.Models;

public partial class ClassStudent
{
    public string ClassId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime EnrolledAt { get; set; }

    /// <summary>ClassRole: student | assistant (null treated as student)</summary>
    public string? ClassRole { get; set; } = "student";
    public virtual Class Class { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
