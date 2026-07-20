using System;

namespace SWP.BLL.DTOs.Quizzes;

public class QuizOptionDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = null!;
    public bool? IsCorrect { get; set; } // Chỉ trả về cho giảng viên, hoặc null đối với học sinh
}
