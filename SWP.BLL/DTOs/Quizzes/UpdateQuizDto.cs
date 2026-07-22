using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Quizzes;

public class UpdateQuizDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? TimeLimit { get; set; }

    public int MaxAttempts { get; set; } = 1;

    public string? PublishDate { get; set; }

    public string? Deadline { get; set; }

    [Required]
    public List<CreateQuizQuestionDto> Questions { get; set; } = new();
}
