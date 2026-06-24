using System;

namespace SWP.BLL.DTOs.Submissions
{
    public class SubmissionDto
    {
        // uniqueidentifier in SQL
        public Guid Id { get; set; }

        // uniqueidentifier in SQL
        public Guid AssignmentId { get; set; }

        public string StudentId { get; set; } = null!;

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

    public class SubmitAssignmentRequestDto
    {
        // uniqueidentifier in SQL
        public Guid AssignmentId { get; set; }

        public string? FileName { get; set; }

        // NOTE: FileUrl does NOT exist in SQL [Submissions] table — removed

        public string? StudentNotes { get; set; }
    }
}
