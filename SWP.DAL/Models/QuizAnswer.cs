using System;

namespace SWP.DAL.Models;

public partial class QuizAnswer
{
    public Guid AttemptId { get; set; }

    public Guid QuestionId { get; set; }

    public Guid SelectedOptionId { get; set; }

    public virtual QuizAttempt Attempt { get; set; } = null!;

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual QuizOption SelectedOption { get; set; } = null!;
}
