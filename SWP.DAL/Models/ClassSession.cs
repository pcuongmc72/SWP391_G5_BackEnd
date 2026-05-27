using System;

namespace SWP.DAL.Models;

public partial class ClassSession
{
    public Guid Id { get; set; }

    public string ClassId { get; set; } = null!;

    public DateOnly SessionDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string Title { get; set; } = null!;

    public string? Detail { get; set; }

    public string? Room { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;
}
