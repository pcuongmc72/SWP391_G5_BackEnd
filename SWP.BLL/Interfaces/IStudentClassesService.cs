using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.BLL.DTOs.Classes;

namespace SWP.BLL.Interfaces;

public interface IStudentClassesService
{
    /// <summary>
    /// Lấy danh sách các lớp học của một sinh viên cụ thể trong một học kỳ (nếu truyền AcademicTermId).
    /// </summary>
    Task<IEnumerable<ClassResponseDto>> GetClassesForStudentAsync(string studentId, Guid? academicTermId = null);

    /// <summary>
    /// Lấy danh sách các buổi học (lộ trình theo tuần) của một lớp học.
    /// </summary>
    Task<IEnumerable<ClassSessionDto>> GetClassSessionsAsync(string classId);

    /// <summary>
    /// Lấy lộ trình học tập (roadmap) của một lớp, nhóm theo Chapter.
    /// Trả về trạng thái hoàn thành (isCompleted) của từng học liệu cho sinh viên đó.
    /// </summary>
    Task<StudentClassRoadmapDto> GetClassRoadmapAsync(string studentId, string classId);

    /// <summary>
    /// Đánh dấu một học liệu là đã hoàn thành (complete) cho sinh viên.
    /// </summary>
    Task<bool> CompleteMaterialAsync(string studentId, Guid materialId);

    /// <summary>
    /// Hủy đánh dấu hoàn thành (uncomplete) một học liệu cho sinh viên.
    /// </summary>
    Task<bool> UncompleteMaterialAsync(string studentId, Guid materialId);

    /// <summary>
    /// Lấy danh sách bài tập của lớp học kèm bài nộp của sinh viên hiện tại.
    /// </summary>
    Task<IEnumerable<SWP.BLL.DTOs.Lecturer.StudentAssignmentDto>> GetStudentAssignmentsAsync(string studentId, string classId);

    /// <summary>
    /// Sinh viên nộp (hoặc nộp lại) bài tập. Tự động ghi đè nếu đã có bài nộp và chưa bị chấm điểm.
    /// </summary>
    Task<SWP.BLL.DTOs.Lecturer.SubmissionDto> SubmitAssignmentAsync(string studentId, string classId, Guid assignmentId, SWP.BLL.DTOs.Lecturer.SubmitAssignmentRequestDto request);

    /// <summary>
    /// Sinh viên gửi câu hỏi cho giảng viên (tùy chọn gắn với bài học cụ thể).
    /// </summary>
    Task<SWP.BLL.DTOs.Lecturer.FeedbackDto> CreateFeedbackAsync(string studentId, string classId, SWP.BLL.DTOs.Lecturer.CreateFeedbackDto request);

    /// <summary>
    /// Sinh viên xem danh sách câu hỏi của mình trong lớp.
    /// </summary>
    Task<IReadOnlyList<SWP.BLL.DTOs.Lecturer.FeedbackDto>> GetMyFeedbacksAsync(string studentId, string classId);

    /// <summary>
    /// Trợ giảng giải đáp câu hỏi của sinh viên.
    /// </summary>
    Task<SWP.BLL.DTOs.Lecturer.FeedbackDto> RespondFeedbackAsAssistantAsync(string assistantId, string classId, Guid feedbackId, SWP.BLL.DTOs.Lecturer.RespondFeedbackDto request);
}
