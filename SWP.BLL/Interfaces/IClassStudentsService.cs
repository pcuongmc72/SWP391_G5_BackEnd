using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.DAL.Models;

namespace SWP.BLL.Interfaces
{
    public interface IClassStudentsService
    {
        Task<IEnumerable<ClassStudent>> GetByClassIdAsync(string classId);
        Task<ClassStudent> EnrollAsync(string classId, string studentId);
        Task<bool> UnenrollAsync(string classId, string studentId);
    }
}
