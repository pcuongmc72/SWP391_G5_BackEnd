using System;

namespace SWP.BLL.DTOs.Submissions
{
    public class SubmissionGradeDetailDto
    {
        public string SubmissionId { get; set; } = null!;
        public string AssignmentId { get; set; } = null!;
        public string AssignmentTitle { get; set; } = null!;
        public string? AssignmentDescription { get; set; }
        public DateOnly? DueDate { get; set; }
        public int MaxPoints { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public string? StudentNotes { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string SubmissionStatus { get; set; } = null!;
        public decimal? Grade { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
    }
}
