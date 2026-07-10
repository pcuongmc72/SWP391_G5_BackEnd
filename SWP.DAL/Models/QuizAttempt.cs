using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class QuizAttempt
{
    public Guid Id { get; set; }

    public Guid QuizId { get; set; }

    public string StudentId { get; set; } = null!;

    public int AttemptNumber { get; set; } // Lượt làm thứ mấy

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public decimal? TotalScore { get; set; } // Điểm tổng của lượt này

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
}
