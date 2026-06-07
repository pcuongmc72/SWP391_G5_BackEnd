using System;

namespace SWP.BLL.DTOs.Assignments
{
    public class AssignmentDto
    {
        public string Id { get; set; } = null!;
        public string ClassId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly? DueDate { get; set; }
        public int MaxPoints { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>Trạng thái nộp bài của sinh viên hiện tại (null nếu chưa nộp)</summary>
        public SubmissionStatusDto? MySubmission { get; set; }
    }

    public class SubmissionStatusDto
    {
        public string Id { get; set; } = null!;
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public string? StudentNotes { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public decimal? Grade { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
    }
}
