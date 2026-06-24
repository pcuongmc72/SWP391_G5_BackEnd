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
    /// Lấy lộ trình học liệu nhóm theo Chapter kèm trạng thái hoàn thành.
    /// </summary>
    Task<StudentClassRoadmapDto> GetClassRoadmapAsync(string studentId, string classId);

    /// <summary>
    /// Đánh dấu đã hoàn thành học liệu cho Sinh viên.
    /// </summary>
    Task<bool> CompleteMaterialAsync(string studentId, Guid materialId);

    /// <summary>
    /// Hủy đánh dấu hoàn thành học liệu cho Sinh viên.
    /// </summary>
    Task<bool> UncompleteMaterialAsync(string studentId, Guid materialId);
}


