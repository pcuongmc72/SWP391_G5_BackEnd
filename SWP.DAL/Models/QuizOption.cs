using System;

namespace SWP.DAL.Models;

public partial class QuizOption
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; } = false; // Có phải đáp án đúng không

    public virtual QuizQuestion Question { get; set; } = null!;
}
