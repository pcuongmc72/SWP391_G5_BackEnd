namespace SWP.DAL.Models;

public partial class Submission
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    public string StudentId { get; set; } = null!;

    public string? FileName { get; set; }

    public string? StudentNotes { get; set; }

    public string Status { get; set; } = "SUBMITTED";

    public decimal? Grade { get; set; }

    public string? Feedback { get; set; }

    public DateTime SubmittedAt { get; set; }

    public DateTime? GradedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
