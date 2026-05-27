namespace SWP.DAL.Models;

public partial class DiscussionThread
{
    public Guid Id { get; set; }

    public string? ClassId { get; set; }

    public string AuthorId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<DiscussionReply> DiscussionReplies { get; set; } = new List<DiscussionReply>();
}
