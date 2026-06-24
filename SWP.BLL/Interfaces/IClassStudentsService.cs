using SWP.BLL.DTOs.ClassStudents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.Interfaces
{
    public interface IClassStudentsService
    {
        Task<IEnumerable<StudentInClassDto>> GetStudentsInClassAsync(string classId);
        Task<StudentInClassDto> AddStudentToClassAsync(string classId, AddStudentRequestDto request);
        Task<bool> RemoveStudentFromClassAsync(string classId, string studentId);
    }
}
