using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Quizzes;

public class CreateQuizQuestionDto
{
    [Required]
    public string QuestionText { get; set; } = null!;

    public decimal Points { get; set; } = 0;

    [Required]
    public List<CreateQuizOptionDto> Options { get; set; } = new();
}
