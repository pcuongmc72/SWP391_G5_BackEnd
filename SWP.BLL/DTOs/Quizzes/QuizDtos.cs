using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Quizzes;

public class CreateQuizDto
{
    [Required]
    public string ClassId { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? TimeLimit { get; set; } // Phút, null là không giới hạn

    public int MaxAttempts { get; set; } = 1;

    [Required]
    public string Chapter { get; set; } = null!; // Tên chương học để tạo LearningMaterial

    public string? Lesson { get; set; } // Tên bài học

    [Required]
    public List<CreateQuizQuestionDto> Questions { get; set; } = new();
}

public class CreateQuizQuestionDto
{
    [Required]
    public string QuestionText { get; set; } = null!;

    public decimal Points { get; set; } = 0;

    [Required]
    public List<CreateQuizOptionDto> Options { get; set; } = new();
}

public class CreateQuizOptionDto
{
    [Required]
    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; } = false;
}

public class UpdateQuizDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? TimeLimit { get; set; }

    public int MaxAttempts { get; set; } = 1;

    [Required]
    public List<CreateQuizQuestionDto> Questions { get; set; } = new();
}

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

public class QuizDetailDto : QuizResponseDto
{
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

public class QuizQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public decimal Points { get; set; }
    public int Order { get; set; }
    public List<QuizOptionDto> Options { get; set; } = new();
}

public class QuizOptionDto
{
    public Guid Id { get; set; }
    public string OptionText { get; set; } = null!;
    public bool? IsCorrect { get; set; } // Chỉ trả về cho giảng viên, hoặc null đối với học sinh
}

public class SubmitQuizDto
{
    [Required]
    public List<SubmitQuizAnswerDto> Answers { get; set; } = new();
}

public class SubmitQuizAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid SelectedOptionId { get; set; }
}

public class QuizResultDto
{
    public Guid AttemptId { get; set; }
    public decimal TotalScore { get; set; }
    public int CorrectAnswersCount { get; set; }
    public int TotalQuestionsCount { get; set; }
    public DateTime SubmittedAt { get; set; }
}

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
