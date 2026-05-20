using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? Major { get; set; }

    public virtual User User { get; set; } = null!;
}
