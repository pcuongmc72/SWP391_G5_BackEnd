using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Blog
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string AuthorId { get; set; } = null!;

    public Guid CourseId { get; set; }

    public string? ClassId { get; set; }

    public bool IsPrivate { get; set; }

    public int Status { get; set; }

    public string? Keywords { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;

    public virtual Class? Class { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
