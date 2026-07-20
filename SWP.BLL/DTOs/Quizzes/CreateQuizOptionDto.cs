using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Quizzes;

public class CreateQuizOptionDto
{
    [Required]
    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; } = false;
}
