using System;

namespace SWP.DAL.Models;

// SQL table: [dbo].[Submissions]
public partial class Submission
{
    // uniqueidentifier NOT NULL DEFAULT (newsequentialid())
    public Guid Id { get; set; }

    // uniqueidentifier NOT NULL  FK → Assignments
    public Guid AssignmentId { get; set; }

    // varchar(20) NOT NULL  FK → Users
    public string StudentId { get; set; } = null!;

    /// <summary>Tên file bài nộp (VD: BaoCao_Nhom1.pdf)</summary>
    // nvarchar(255) NULL
    public string? FileName { get; set; }

    /// <summary>Lời nhắn của sinh viên gửi kèm</summary>
    // nvarchar(max) NULL
    public string? StudentNotes { get; set; }

    /// <summary>Thời điểm nộp bài</summary>
    // datetime2(0) NOT NULL DEFAULT (sysdatetime())
    public DateTime SubmittedAt { get; set; }

    /// <summary>Trạng thái: SUBMITTED | GRADED  — SQL CHECK constraint</summary>
    // varchar(20) NOT NULL DEFAULT ('SUBMITTED')  CHECK: 'GRADED'|'SUBMITTED'
    public string Status { get; set; } = "SUBMITTED";

    /// <summary>Điểm số (null nếu chưa chấm)</summary>
    // decimal(5,2) NULL
    public decimal? Grade { get; set; }

    /// <summary>Nhận xét của giảng viên</summary>
    // nvarchar(max) NULL
    public string? Feedback { get; set; }

    /// <summary>Thời điểm giảng viên chấm điểm</summary>
    // datetime2(0) NULL
    public DateTime? GradedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
