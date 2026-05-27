namespace SWP.BLL.DTOs.Lecturer;

public class LecturerClassListItemDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string CourseCode { get; set; } = null!;
    public string CourseName { get; set; } = null!;
    public string TermName { get; set; } = null!;
    public int StudentCount { get; set; }
    public int SessionCount { get; set; }
}

public class LecturerClassDetailDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool AllowReviewAfterEnd { get; set; }
    public Guid CourseId { get; set; }
    public string CourseCode { get; set; } = null!;
    public string CourseName { get; set; } = null!;
    public string? CourseDescription { get; set; }
    public Guid AcademicTermId { get; set; }
    public string TermName { get; set; } = null!;
    public DateOnly TermStartDate { get; set; }
    public DateOnly TermEndDate { get; set; }
    public int StudentCount { get; set; }
}

public class ClassSessionDto
{
    public Guid Id { get; set; }
    public string ClassId { get; set; } = null!;
    public DateOnly SessionDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string Title { get; set; } = null!;
    public string? Detail { get; set; }
    public string? Room { get; set; }
}

public class UpsertClassSessionDto
{
    public DateOnly SessionDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string Title { get; set; } = null!;
    public string? Detail { get; set; }
    public string? Room { get; set; }
}

public class ClassStudentDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}

public class MaterialDto
{
    public Guid Id { get; set; }
    public string ClassId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public string? FileUrl { get; set; }
    public string? FileSize { get; set; }
    public DateOnly UploadedAt { get; set; }
    public List<string> CompletedByUsers { get; set; } = new();
}

public class UpsertMaterialDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = "video";
    public string? FileUrl { get; set; }
    public string? FileSize { get; set; }
}

public class AssignmentDto
{
    public Guid Id { get; set; }
    public string ClassId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal MaxPoints { get; set; }
    public int SubmissionCount { get; set; }
}

public class UpsertAssignmentDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal MaxPoints { get; set; } = 10;
}

public class SubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string StudentId { get; set; } = null!;
    public string? StudentName { get; set; }
    public string? FileName { get; set; }
    public string? StudentNotes { get; set; }
    public string Status { get; set; } = null!;
    public decimal? Grade { get; set; }
    public string? Feedback { get; set; }
    public string SubmittedAt { get; set; } = null!;
    public string? GradedAt { get; set; }
}

public class GradeSubmissionDto
{
    public decimal Grade { get; set; }
    public string Feedback { get; set; } = null!;
}

public class FeedbackDto
{
    public Guid Id { get; set; }
    public string? ClassId { get; set; }
    public string SenderId { get; set; } = null!;
    public string? SenderName { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Response { get; set; }
    public string CreatedAt { get; set; } = null!;
    public string? RespondedAt { get; set; }
}

public class RespondFeedbackDto
{
    public string Response { get; set; } = null!;
}

public class DiscussionReplyDto
{
    public Guid Id { get; set; }
    public string AuthorId { get; set; } = null!;
    public string? AuthorName { get; set; }
    public string Content { get; set; } = null!;
    public string CreatedAt { get; set; } = null!;
}

public class DiscussionThreadDto
{
    public Guid Id { get; set; }
    public string? ClassId { get; set; }
    public string AuthorId { get; set; } = null!;
    public string? AuthorName { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string CreatedAt { get; set; } = null!;
    public List<DiscussionReplyDto> Replies { get; set; } = new();
}

public class UpsertThreadDto
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
}

public class CreateReplyDto
{
    public string Content { get; set; } = null!;
}

public class LecturerClassWorkspaceDto
{
    public LecturerClassDetailDto Class { get; set; } = null!;
    public List<ClassStudentDto> Students { get; set; } = new();
    public List<MaterialDto> Materials { get; set; } = new();
    public List<AssignmentDto> Assignments { get; set; } = new();
    public List<SubmissionDto> Submissions { get; set; } = new();
    public List<FeedbackDto> Feedbacks { get; set; } = new();
    public List<DiscussionThreadDto> Threads { get; set; } = new();
    public List<ClassSessionDto> Sessions { get; set; } = new();
}
