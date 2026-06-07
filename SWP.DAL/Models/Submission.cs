using System;

namespace SWP.DAL.Models;

public partial class Submission
{
    public string Id { get; set; } = null!;

    public string AssignmentId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    /// <summary>Tên file bài nộp (VD: BaoCao_Nhom1.pdf)</summary>
    public string? FileName { get; set; }

    /// <summary>URL file bài nộp (có thể là đường dẫn lưu trữ)</summary>
    public string? FileUrl { get; set; }

    /// <summary>Lời nhắn của sinh viên gửi kèm</summary>
    public string? StudentNotes { get; set; }

    /// <summary>Thời điểm nộp bài</summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>Trạng thái: SUBMITTED | GRADED | LATE</summary>
    public string Status { get; set; } = "SUBMITTED";

    /// <summary>Điểm số (null nếu chưa chấm)</summary>
    public decimal? Grade { get; set; }

    /// <summary>Nhận xét của giảng viên</summary>
    public string? Feedback { get; set; }

    /// <summary>Thời điểm giảng viên chấm điểm</summary>
    public DateTime? GradedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
