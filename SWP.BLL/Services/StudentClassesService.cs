using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Classes;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class StudentClassesService : IStudentClassesService
{
    private readonly FlippedClassroomContext _context;

    public StudentClassesService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassResponseDto>> GetClassesForStudentAsync(string studentId, Guid? academicTermId = null)
    {
        // Truy vấn trực tiếp từ Classes để đảm bảo các Include không bị bỏ qua
        var query = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.AcademicTerm)
            .Include(c => c.Lecturer)
            .Include(c => c.ClassStudents)
            // Lọc các lớp học mà danh sách ClassStudents có chứa StudentId này
            .Where(c => c.ClassStudents.Any(cs => cs.StudentId == studentId))
            .AsQueryable();

        if (academicTermId.HasValue)
        {
            query = query.Where(c => c.AcademicTermId == academicTermId.Value);
        }

        var classes = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return classes.Select(MapToDto);
    }


    // Hàm chuyển đổi Class Entity sang ClassResponseDto giống cấu trúc dự án của bạn
    private static ClassResponseDto MapToDto(Class classEntity) => new()
    {
        Id = classEntity.Id,
        CourseId = classEntity.CourseId,
        CourseCode = classEntity.Course?.Code ?? "N/A",
        CourseName = classEntity.Course?.Name ?? "N/A",
        AcademicTermId = classEntity.AcademicTermId,
        TermCode = classEntity.AcademicTerm?.TermCode ?? "N/A",
        LecturerId = classEntity.LecturerId,
        LecturerName = classEntity.Lecturer?.FullName ?? "N/A",
        StartDate = classEntity.StartDate ?? classEntity.AcademicTerm?.StartDate,
        EndDate = classEntity.EndDate ?? classEntity.AcademicTerm?.EndDate,
        TotalStudents = classEntity.ClassStudents?.Count ?? 0,
        AllowReviewAfterEnd = classEntity.AllowReviewAfterEnd,
        CreatedAt = classEntity.CreatedAt
    };
}
