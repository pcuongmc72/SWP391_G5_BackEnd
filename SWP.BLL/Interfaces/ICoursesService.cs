using SWP.BLL.DTOs.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP.BLL.Interfaces
{
    public interface ICoursesService
    {
        Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync();
    Task<IEnumerable<CourseResponseDto>> GetCoursesByUserAsync(string userId, string role);
        Task<CourseResponseDto> GetCourseByIdAsync(Guid id);
        Task<CourseResponseDto> CreateCourseAsync(CourseRequestDto request);
        Task<CourseResponseDto> UpdateCourseAsync(Guid id, CourseRequestDto request);
        Task<bool> DeleteCourseAsync(Guid id);
    }
}
