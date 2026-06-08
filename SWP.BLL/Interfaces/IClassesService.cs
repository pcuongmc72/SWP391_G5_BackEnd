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
        Task<IEnumerable<ClassResponseDto>> GetClassesByStudentAsync(string studentId, Guid? academicTermId = null);
        Task<IEnumerable<ClassResponseDto>> GetClassesByUserAsync(string userId, string role, Guid? academicTermId = null);
        Task<ClassResponseDto> GetClassByIdAsync(string id);
        Task<ClassResponseDto> CreateClassAsync(ClassRequestDto request);
        Task<ClassResponseDto> UpdateClassAsync(string id, ClassRequestDto request);
        Task<bool> DeleteClassAsync(string id);
    }
}
