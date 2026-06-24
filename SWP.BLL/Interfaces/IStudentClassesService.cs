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
}

