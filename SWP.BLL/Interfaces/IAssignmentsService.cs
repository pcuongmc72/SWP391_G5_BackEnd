using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.BLL.DTOs.Assignments;

namespace SWP.BLL.Interfaces
{
    public interface IAssignmentsService
    {
        /// <summary>
        /// Lấy danh sách bài tập của một lớp học, kèm trạng thái nộp bài của sinh viên hiện tại.
        /// </summary>
        Task<IEnumerable<AssignmentDto>> GetAssignmentsByClassAsync(string classId, string studentId);

        /// <summary>
        /// Lấy chi tiết một bài tập, kèm trạng thái nộp bài của sinh viên hiện tại.
        /// </summary>
        Task<AssignmentDto> GetAssignmentByIdAsync(string assignmentId, string studentId);
    }
}
