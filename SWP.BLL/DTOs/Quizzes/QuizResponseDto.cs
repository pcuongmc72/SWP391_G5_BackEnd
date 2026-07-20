using System;

namespace SWP.BLL.DTOs.Quizzes;

public class QuizResponseDto
{
    public Guid Id { get; set; }
    public string ClassId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? TimeLimit { get; set; }
    public int MaxAttempts { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsDisabled { get; set; }
}
