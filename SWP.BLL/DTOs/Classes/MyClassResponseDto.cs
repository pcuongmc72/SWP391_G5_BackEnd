using System;

namespace SWP.BLL.DTOs.Classes
{
    /// <summary>
    /// DTO trả về danh sách lớp học của sinh viên đang đăng nhập.
    /// Bổ sung các trường phục vụ hiển thị card lớp học trên Student Dashboard.
    /// </summary>
    public class MyClassResponseDto
    {
        public string Id { get; set; } = null!;

        // Course info
        public Guid CourseId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;

        // Term info
        public Guid AcademicTermId { get; set; }
        public string TermCode { get; set; } = null!;
        public string TermName { get; set; } = null!;   // VD: "Học kỳ 2 - 2024-2025"
        public DateOnly? TermStartDate { get; set; }
        public DateOnly? TermEndDate { get; set; }

        // Lecturer info
        public string? LecturerId { get; set; }
        public string LecturerName { get; set; } = null!;
        public string? LecturerEmail { get; set; }

        // Class info
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool AllowReviewAfterEnd { get; set; }
        public int TotalStudents { get; set; }

        // Enrollment info
        public DateTime EnrolledAt { get; set; }

        public int MaterialProgress { get; set; }

        // Grade-related summary info
        public int GradedAssignmentsCount { get; set; }
        public decimal? AverageGrade { get; set; }
        public string? LearningStatus { get; set; }
    }
}
