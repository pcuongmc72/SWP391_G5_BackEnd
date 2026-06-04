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

public class ClassesService : IClassesService
{
    private readonly FlippedClassroomContext _context;

    public ClassesService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync(Guid? academicTermId = null)
    {
        var query = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.AcademicTerm)
            .Include(c => c.Lecturer)
            .Include(c => c.ClassStudents)
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

    public async Task<ClassResponseDto> GetClassByIdAsync(string id)
    {
        var classEntity = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.AcademicTerm)
            .Include(c => c.Lecturer)
            .Include(c => c.ClassStudents)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classEntity == null)
            throw new KeyNotFoundException("Không tìm thấy lớp học.");

        return MapToDto(classEntity);
    }

    public async Task<ClassResponseDto> CreateClassAsync(ClassRequestDto request)
    {
        if (await _context.Classes.AnyAsync(c => c.Id.ToUpper() == request.Id.ToUpper()))
            throw new InvalidOperationException($"Mã lớp '{request.Id.ToUpper()}' đã tồn tại trong hệ thống.");

        await ValidateForeignKeysAsync(request.CourseId, request.AcademicTermId, request.LecturerId);

        // Kiểm tra học kỳ đã kết thúc chưa
        var term = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.Id == request.AcademicTermId);
        if (term != null && term.EndDate < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new InvalidOperationException("Học kỳ này đã kết thúc. Không thể tạo thêm lớp học mới.");
        }

        var newClass = new Class
        {
            Id = request.Id.ToUpper(),
            CourseId = request.CourseId,
            AcademicTermId = request.AcademicTermId,
            LecturerId = request.LecturerId,
            AllowReviewAfterEnd = request.AllowReviewAfterEnd,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.Classes.Add(newClass);
        await _context.SaveChangesAsync();

        return await GetClassByIdAsync(newClass.Id);
    }

    public async Task<ClassResponseDto> UpdateClassAsync(string id, ClassRequestDto request)
    {
        var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == id);
        if (classEntity == null)
            throw new KeyNotFoundException("Không tìm thấy lớp học cần cập nhật.");

        // Kiểm tra lớp học đã kết thúc chưa
        if (classEntity.EndDate.HasValue && classEntity.EndDate.Value < DateOnly.FromDateTime(DateTime.Now))
            throw new InvalidOperationException("Lớp học này đã kết thúc. Không thể thực hiện chỉnh sửa hoặc cập nhật.");

        if (request.Id.ToUpper() != id.ToUpper())
        {
            // Kiểm tra trùng mã lớp mới
            if (await _context.Classes.AnyAsync(c => c.Id.ToUpper() == request.Id.ToUpper()))
                throw new InvalidOperationException($"Mã lớp mới '{request.Id.ToUpper()}' đã tồn tại trong hệ thống.");

            await ValidateForeignKeysAsync(request.CourseId, request.AcademicTermId, request.LecturerId);

            // Vì ID là khóa chính, ta phải xóa thực thể cũ và thêm thực thể mới
            _context.Classes.Remove(classEntity);
            await _context.SaveChangesAsync();

            var newClass = new Class
            {
                Id = request.Id.ToUpper(),
                CourseId = request.CourseId,
                AcademicTermId = request.AcademicTermId,
                LecturerId = request.LecturerId,
                AllowReviewAfterEnd = request.AllowReviewAfterEnd,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();

            return await GetClassByIdAsync(newClass.Id);
        }

        await ValidateForeignKeysAsync(request.CourseId, request.AcademicTermId, request.LecturerId);

        classEntity.CourseId = request.CourseId;
        classEntity.AcademicTermId = request.AcademicTermId;
        classEntity.LecturerId = request.LecturerId;
        classEntity.AllowReviewAfterEnd = request.AllowReviewAfterEnd;
        classEntity.StartDate = request.StartDate;
        classEntity.EndDate = request.EndDate;

        await _context.SaveChangesAsync();

        return await GetClassByIdAsync(id);
    }

    public async Task<bool> DeleteClassAsync(string id)
    {
        var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == id);
        if (classEntity == null)
            throw new KeyNotFoundException("Không tìm thấy lớp học.");

        if (!string.IsNullOrEmpty(classEntity.LecturerId))
            throw new InvalidOperationException("Không thể xóa lớp học này vì đang có giảng viên phụ trách.");

        bool hasStudents = await _context.ClassStudents.AnyAsync(cs => cs.ClassId == id);
        if (hasStudents)
            throw new InvalidOperationException("Không thể xóa lớp học này vì đang có sinh viên theo học.");

        _context.Classes.Remove(classEntity);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task ValidateForeignKeysAsync(Guid courseId, Guid termId, string? lecturerId)
    {
        if (!await _context.Courses.AnyAsync(c => c.Id == courseId))
            throw new InvalidOperationException("Môn học được chọn không tồn tại.");

        if (!await _context.AcademicTerms.AnyAsync(t => t.Id == termId))
            throw new InvalidOperationException("Kỳ học được chọn không tồn tại.");

        if (!string.IsNullOrEmpty(lecturerId))
        {
            var lecturer = await _context.Users.FirstOrDefaultAsync(u => u.Id == lecturerId);
            if (lecturer == null)
                throw new InvalidOperationException("Giảng viên được phân công không tồn tại.");

            if (lecturer.Role != null && lecturer.Role.ToLower() != "lecturer")
                throw new InvalidOperationException("Tài khoản được phân công không phải là Giảng viên.");
        }
    }

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