using System;

namespace SWP.BLL.DTOs.Blogs
{
    public class BlogResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string AuthorId { get; set; } = null!;
        public string AuthorFullName { get; set; } = null!;
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string? ClassId { get; set; }
        public bool IsPrivate { get; set; }
        public int Status { get; set; }
        public string? Keywords { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Role { get; set; }
    }
}
