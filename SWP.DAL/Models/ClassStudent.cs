using System;

namespace SWP.DAL.Models;

public partial class ClassStudent
{
    public string ClassId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime EnrolledAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
