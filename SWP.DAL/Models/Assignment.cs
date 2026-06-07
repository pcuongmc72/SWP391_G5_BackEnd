using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Assignment
{
    public string Id { get; set; } = null!;

    public string ClassId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? DueDate { get; set; }

    public int MaxPoints { get; set; } = 100;

    public DateTime CreatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
