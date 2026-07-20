using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Quizzes;

public class SubmitQuizDto
{
    [Required]
    public List<SubmitQuizAnswerDto> Answers { get; set; } = new();
}
