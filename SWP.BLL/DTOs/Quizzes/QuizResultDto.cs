using System;

namespace SWP.BLL.DTOs.Quizzes;

public class QuizResultDto
{
    public Guid AttemptId { get; set; }
    public decimal TotalScore { get; set; }
    public int CorrectAnswersCount { get; set; }
    public int TotalQuestionsCount { get; set; }
    public DateTime SubmittedAt { get; set; }
}
