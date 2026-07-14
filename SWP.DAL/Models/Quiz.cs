using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Quiz
{
    public Guid Id { get; set; }

    public string ClassId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? TimeLimit { get; set; } // Phút, null là không giới hạn

    public int MaxAttempts { get; set; } = 1; // Số lần làm bài tối đa

    public string CreatedBy { get; set; } = null!; // Giảng viên tạo

    public DateTime CreatedAt { get; set; }

    public bool IsDisabled { get; set; } = false;

    public virtual Class Class { get; set; } = null!;

    public virtual User Creator { get; set; } = null!;

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
