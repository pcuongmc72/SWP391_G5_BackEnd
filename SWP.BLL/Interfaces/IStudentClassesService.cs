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
}
