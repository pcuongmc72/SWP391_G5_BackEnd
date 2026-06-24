using System;

namespace SWP.BLL.DTOs.Assignments
{
    public class AssignmentDto
    {
        // uniqueidentifier in SQL
        public Guid Id { get; set; }

        public string ClassId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        // date NOT NULL in SQL
        public DateOnly DueDate { get; set; }

        // decimal(5,2) NOT NULL DEFAULT ((10)) in SQL
        public decimal MaxPoints { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>Trạng thái nộp bài của sinh viên hiện tại (null nếu chưa nộp)</summary>
        public SubmissionStatusDto? MySubmission { get; set; }
    }

    public class SubmissionStatusDto
    {
        // uniqueidentifier in SQL
        public Guid Id { get; set; }

        public string? FileName { get; set; }

        // NOTE: FileUrl does NOT exist in SQL [Submissions] table — removed

        public string? StudentNotes { get; set; }

        public DateTime SubmittedAt { get; set; }

        // CHECK constraint: 'SUBMITTED' | 'GRADED'
        public string Status { get; set; } = null!;

        public decimal? Grade { get; set; }

        public string? Feedback { get; set; }

        public DateTime? GradedAt { get; set; }
    }
}
