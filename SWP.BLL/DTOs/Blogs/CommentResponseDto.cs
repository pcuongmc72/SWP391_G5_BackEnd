using System;

namespace SWP.BLL.DTOs.Blogs
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }
        public string AuthorId { get; set; } = null!;
        public string AuthorFullName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Role { get; set; }
    }
}
