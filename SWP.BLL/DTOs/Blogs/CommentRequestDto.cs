using System;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Blogs
{
    public class CommentRequestDto
    {
        [Required]
        public Guid BlogId { get; set; }

        [Required]
        public string AuthorId { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;
    }
}
