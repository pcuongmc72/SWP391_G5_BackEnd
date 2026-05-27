namespace SWP.DAL.Models;

public partial class DiscussionReply
{
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }

    public string AuthorId { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual DiscussionThread Thread { get; set; } = null!;

    public virtual User Author { get; set; } = null!;
}
