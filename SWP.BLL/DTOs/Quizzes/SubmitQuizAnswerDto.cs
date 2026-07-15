using System;

namespace SWP.BLL.DTOs.Quizzes;

public class SubmitQuizAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}
