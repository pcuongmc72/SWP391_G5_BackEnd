using SWP.BLL.DTOs.Lecturer;

namespace SWP.BLL.Interfaces;

public interface ILecturerService
{
    Task<IReadOnlyList<LecturerClassListItemDto>> GetMyClassesAsync(string lecturerId);
    Task<LecturerClassDetailDto> GetClassDetailAsync(string lecturerId, string classId);
    Task<LecturerClassWorkspaceDto> GetClassWorkspaceAsync(string lecturerId, string classId);

    Task<IReadOnlyList<ClassStudentDto>> GetClassStudentsAsync(string lecturerId, string classId);

    Task<IReadOnlyList<ClassSessionDto>> GetSessionsAsync(string lecturerId, string classId, DateOnly? from, DateOnly? to);
    Task<ClassSessionDto> GetSessionAsync(string lecturerId, string classId, Guid sessionId);
    Task<ClassSessionDto> CreateSessionAsync(string lecturerId, string classId, UpsertClassSessionDto request);
    Task<ClassSessionDto> UpdateSessionAsync(string lecturerId, string classId, Guid sessionId, UpsertClassSessionDto request);
    Task DeleteSessionAsync(string lecturerId, string classId, Guid sessionId);

    Task<IReadOnlyList<MaterialDto>> GetMaterialsAsync(string lecturerId, string classId);
    Task<MaterialDto> CreateMaterialAsync(string lecturerId, string classId, UpsertMaterialDto request);
    Task<MaterialDto> UpdateMaterialAsync(string lecturerId, string classId, Guid materialId, UpsertMaterialDto request);
    Task DeleteMaterialAsync(string lecturerId, string classId, Guid materialId);
    Task MarkMaterialCompleteAsync(string lecturerId, string classId, Guid materialId);

    Task<IReadOnlyList<AssignmentDto>> GetAssignmentsAsync(string lecturerId, string classId);
    Task<AssignmentDto> CreateAssignmentAsync(string lecturerId, string classId, UpsertAssignmentDto request);
    Task<AssignmentDto> UpdateAssignmentAsync(string lecturerId, string classId, Guid assignmentId, UpsertAssignmentDto request);
    Task DeleteAssignmentAsync(string lecturerId, string classId, Guid assignmentId);

    Task<IReadOnlyList<SubmissionDto>> GetSubmissionsAsync(string lecturerId, string classId);
    Task<SubmissionDto> GradeSubmissionAsync(string lecturerId, string classId, Guid submissionId, GradeSubmissionDto request);

    Task<IReadOnlyList<FeedbackDto>> GetFeedbacksAsync(string lecturerId, string classId);
    Task<FeedbackDto> RespondFeedbackAsync(string lecturerId, string classId, Guid feedbackId, RespondFeedbackDto request);

    Task<IReadOnlyList<DiscussionThreadDto>> GetThreadsAsync(string lecturerId, string classId);
    Task<DiscussionThreadDto> CreateThreadAsync(string lecturerId, string classId, UpsertThreadDto request);
    Task<DiscussionReplyDto> CreateReplyAsync(string lecturerId, string classId, Guid threadId, CreateReplyDto request);
}
