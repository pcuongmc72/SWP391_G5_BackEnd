namespace SWP.DAL.Models;

public partial class Assignment
{
    public Guid Id { get; set; }

    public string ClassId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal MaxPoints { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
