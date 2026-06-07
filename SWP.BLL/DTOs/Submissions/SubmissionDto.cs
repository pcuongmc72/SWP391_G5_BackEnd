using System;

namespace SWP.BLL.DTOs.Submissions
{
    public class SubmissionDto
    {
        public string Id { get; set; } = null!;
        public string AssignmentId { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public string? StudentNotes { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = null!;
        public decimal? Grade { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
    }

    public class SubmitAssignmentRequestDto
    {
        public string AssignmentId { get; set; } = null!;
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public string? StudentNotes { get; set; }
    }
}
