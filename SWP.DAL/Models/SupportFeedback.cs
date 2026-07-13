namespace SWP.DAL.Models;

public partial class SupportFeedback
{
    public Guid Id { get; set; }

    public string? ClassId { get; set; }

    public string SenderId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Status { get; set; } = "OPEN";

    public string? Response { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public string? AnsweredByUserId { get; set; }

    public virtual User Sender { get; set; } = null!;

    public virtual User? AnsweredBy { get; set; }
}
