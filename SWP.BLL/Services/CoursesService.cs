using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Courses;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class CoursesService : ICoursesService
{
    private readonly FlippedClassroomContext _context;

    public CoursesService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync()
    {
        var courses = await _context.Courses
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return courses.Select(MapToDto);
    }

    public async Task<IEnumerable<CourseResponseDto>> GetCoursesByUserAsync(string userId, string role)
    {
        IQueryable<Class> classQuery = _context.Classes.AsQueryable();

        if (role.ToLower() == "student")
        {
            var enrolledClassIds = await _context.ClassStudents
                .Where(cs => cs.StudentId == userId)
                .Select(cs => cs.ClassId)
                .ToListAsync();
            classQuery = classQuery.Where(c => enrolledClassIds.Contains(c.Id));
        }
        else if (role.ToLower() == "lecturer")
        {
            classQuery = classQuery.Where(c => c.LecturerId == userId);
        }
        else
        {
            // For admin or others, maybe return all or empty. 
            // Applying role filter here assumes we only call this for students/lecturers.
            return await GetAllCoursesAsync();
        }

        var courseIds = await classQuery.Select(c => c.CourseId).Distinct().ToListAsync();
        
        var courses = await _context.Courses
            .Where(c => courseIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return courses.Select(MapToDto);
    }

    public async Task<CourseResponseDto> GetCourseByIdAsync(Guid id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null)
            throw new KeyNotFoundException("Không tìm thấy môn học.");

        return MapToDto(course);
    }

    public async Task<CourseResponseDto> CreateCourseAsync(CourseRequestDto request)
    {
        if (await _context.Courses.AnyAsync(c => c.Code.ToLower() == request.Code.ToLower()))
            throw new InvalidOperationException($"Mã môn học '{request.Code.ToUpper()}' đã tồn tại.");

        var newCourse = new Course
        {
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description
        };

        _context.Courses.Add(newCourse);
        await _context.SaveChangesAsync();

        return MapToDto(newCourse);
    }

    public async Task<CourseResponseDto> UpdateCourseAsync(Guid id, CourseRequestDto request)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null)
            throw new KeyNotFoundException("Không tìm thấy môn học cần cập nhật.");

        if (course.Code.ToUpper() != request.Code.ToUpper() &&
            await _context.Courses.AnyAsync(c => c.Code.ToLower() == request.Code.ToLower()))
        {
            throw new InvalidOperationException($"Mã môn học '{request.Code.ToUpper()}' đã được sử dụng.");
        }

        course.Code = request.Code.ToUpper();
        course.Name = request.Name;
        course.Description = request.Description;

        await _context.SaveChangesAsync();
        return MapToDto(course);
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null)
            throw new KeyNotFoundException("Không tìm thấy môn học.");

        bool hasClasses = await _context.Classes.AnyAsync(c => c.CourseId == id);
        if (hasClasses)
            throw new InvalidOperationException("Không thể xóa môn học này vì đang có lớp học tham chiếu tới.");

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return true;
    }

    private static CourseResponseDto MapToDto(Course course) => new()
    {
        Id = course.Id,
        Code = course.Code,
        Name = course.Name,
        Description = course.Description,
        CreatedAt = course.CreatedAt
    };
}