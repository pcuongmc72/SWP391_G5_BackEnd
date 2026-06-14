using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWP.DAL.Models;

namespace SWP.BLL.Interfaces
{
    public interface ICoursesService
    {
        Task<IEnumerable<Course>> GetAllAsync();
        Task<Course?> GetByIdAsync(Guid id);
        Task<Course> CreateAsync(Course course);
        Task<Course> UpdateAsync(Guid id, Course course);
        Task<bool> DeleteAsync(Guid id);
    }
}
