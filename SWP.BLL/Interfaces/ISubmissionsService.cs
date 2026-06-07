using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.BLL.DTOs.Submissions;

namespace SWP.BLL.Interfaces
{
    public interface ISubmissionsService
    {
        /// <summary>
        /// Lấy danh sách bài nộp của sinh viên hiện tại (có thể lọc theo classId).
        /// </summary>
        Task<IEnumerable<SubmissionDto>> GetMySubmissionsAsync(string studentId, string? classId = null);

        /// <summary>
        /// Nộp bài (hoặc nộp lại).
        /// Nếu đã có bài nộp cho assignment này → cập nhật, nếu chưa → tạo mới.
        /// </summary>
        Task<SubmissionDto> SubmitAssignmentAsync(string studentId, SubmitAssignmentRequestDto request);
        Task<SubmissionGradeDetailDto> GetSubmissionGradeDetailAsync(string submissionId, string studentId);
    }
}
