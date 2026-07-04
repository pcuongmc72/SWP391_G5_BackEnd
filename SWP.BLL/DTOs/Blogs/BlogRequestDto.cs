using System;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Blogs
{
    public class BlogRequestDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [Required]
        public string AuthorId { get; set; } = null!;

        [Required]
        public Guid CourseId { get; set; }

        public string? ClassId { get; set; }

        public bool IsPrivate { get; set; }

        public string? Keywords { get; set; }
    }
}
