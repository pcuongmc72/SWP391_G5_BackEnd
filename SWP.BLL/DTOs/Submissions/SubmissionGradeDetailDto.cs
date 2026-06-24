using System;

namespace SWP.BLL.DTOs.Submissions
{
    public class SubmissionGradeDetailDto
    {
        // uniqueidentifier in SQL
        public Guid SubmissionId { get; set; }

        // uniqueidentifier in SQL
        public Guid AssignmentId { get; set; }

        public string AssignmentTitle { get; set; } = null!;

        public string? AssignmentDescription { get; set; }

        // date NOT NULL in SQL
        public DateOnly DueDate { get; set; }

        // decimal(5,2) in SQL
        public decimal MaxPoints { get; set; }

        public string? FileName { get; set; }

        // NOTE: FileUrl does NOT exist in SQL [Submissions] table — removed

        public string? StudentNotes { get; set; }

        public DateTime SubmittedAt { get; set; }

        // CHECK constraint: 'SUBMITTED' | 'GRADED'
        public string SubmissionStatus { get; set; } = null!;

        public decimal? Grade { get; set; }

        public string? Feedback { get; set; }

        public DateTime? GradedAt { get; set; }
    }
}
