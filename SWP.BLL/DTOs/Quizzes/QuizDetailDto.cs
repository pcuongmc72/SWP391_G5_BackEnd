using System.Collections.Generic;

namespace SWP.BLL.DTOs.Quizzes;

public class QuizDetailDto : QuizResponseDto
{
    public List<QuizQuestionDto> Questions { get; set; } = new();
}
