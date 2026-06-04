using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Comment
{
    public Guid Id { get; set; }

    public Guid BlogId { get; set; }

    public string AuthorId { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual User Author { get; set; } = null!;
}
