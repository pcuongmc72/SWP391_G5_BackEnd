using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Quizzes;

public class CreateQuizDto
{
    [Required]
    public string ClassId { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? TimeLimit { get; set; } // Phút, null là không giới hạn (Dùng int? thay vì int để cho phép giá trị null)

    public int MaxAttempts { get; set; } = 1;

    [Required]
    public string Chapter { get; set; } = null!; // Tên chương học để tạo LearningMaterial

    public string? Lesson { get; set; } // Tên bài học

    public string? PublishDate { get; set; }

    public string? Deadline { get; set; }

    [Required]
    public List<CreateQuizQuestionDto> Questions { get; set; } = new();
}
