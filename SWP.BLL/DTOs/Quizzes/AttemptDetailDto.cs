using System;
using System.Collections.Generic;

namespace SWP.BLL.DTOs.Quizzes;

/// <summary>
/// Chi tiết đáp án sinh viên đã chọn cho 1 câu hỏi trong 1 lượt thi
/// </summary>
public class AttemptAnswerDetailDto
{
    public Guid QuestionId { get; set; }
    public int QuestionOrder { get; set; }
    public string QuestionText { get; set; } = null!;
    public decimal Points { get; set; }
    public bool IsCorrect { get; set; } // Câu này có đúng không (đúng hoàn toàn)

    public List<AttemptOptionDetailDto> Options { get; set; } = new();
}

public class AttemptOptionDetailDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = null!;
    public bool IsCorrect { get; set; }       // Đây có phải đáp án đúng không
    public bool WasSelected { get; set; }     // Sinh viên có chọn cái này không
}

/// <summary>
/// Toàn bộ chi tiết 1 lượt thi, bao gồm từng câu hỏi và đáp án sinh viên đã chọn
/// </summary>
public class AttemptDetailDto
{
    public Guid AttemptId { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }

    public List<AttemptAnswerDetailDto> Questions { get; set; } = new();
}
