using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class QuizQuestion
{
    public Guid Id { get; set; }

    public Guid QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public decimal Points { get; set; } = 0; // Điểm câu hỏi này

    public int Order { get; set; } // Thứ tự câu hỏi

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizOption> QuizOptions { get; set; } = new List<QuizOption>();
}
