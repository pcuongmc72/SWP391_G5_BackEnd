using System;

namespace SWP.BLL.DTOs.Quizzes;

public class QuizAttemptDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public string StudentId { get; set; } = null!;
    public string StudentFullName { get; set; } = null!;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal? TotalScore { get; set; }
}
