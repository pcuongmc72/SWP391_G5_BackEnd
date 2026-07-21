using System;
using System.Collections.Generic;

namespace SWP.BLL.DTOs.Quizzes;

public class QuizQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public decimal Points { get; set; }
    public int Order { get; set; }
    public int MaxSelections { get; set; }
    public List<QuizOptionDto> Options { get; set; } = new();
}
