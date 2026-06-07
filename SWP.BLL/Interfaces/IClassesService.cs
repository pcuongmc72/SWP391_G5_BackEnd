using SWP.BLL.DTOs.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.Interfaces
{
    public interface IClassesService
    {
        Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync(Guid? academicTermId = null);
        Task<ClassResponseDto> GetClassByIdAsync(string id);
        Task<ClassResponseDto> CreateClassAsync(ClassRequestDto request);
        Task<ClassResponseDto> UpdateClassAsync(string id, ClassRequestDto request);
        Task<bool> DeleteClassAsync(string id);

        /// <summary>
        /// Lấy danh sách lớp học mà sinh viên (studentId) đang tham gia.
        /// Hỗ trợ filter theo academicTermId và năm học (year).
        /// </summary>
        Task<IEnumerable<MyClassResponseDto>> GetMyClassesAsync(string studentId, Guid? academicTermId = null, string? year = null);
        Task<IEnumerable<MyClassResponseDto>> GetMyClassesGradesAsync(string studentId);
    }
}
