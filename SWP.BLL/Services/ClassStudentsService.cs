using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.ClassStudents;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class ClassStudentsService : IClassStudentsService
{
    private readonly FlippedClassroomContext _context;

    public ClassStudentsService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentInClassDto>> GetStudentsInClassAsync(string classId)
    {
        // 1. Kiểm tra lớp học có tồn tại không
        if (!await _context.Classes.AnyAsync(c => c.Id == classId))
            throw new KeyNotFoundException("Không tìm thấy lớp học.");

        // 2. Lấy danh sách, Join với bảng Users để lấy Tên và Email
        var students = await _context.ClassStudents
            .Include(cs => cs.Student)
            .Where(cs => cs.ClassId == classId)
            .OrderByDescending(cs => cs.EnrolledAt)
            .ToListAsync();

        return students.Select(cs => new StudentInClassDto
        {
            StudentId = cs.StudentId,
            FullName = cs.Student?.FullName ?? "N/A",
            Email = cs.Student?.Email ?? "N/A",
            AvatarUrl = cs.Student?.AvatarUrl,
            EnrolledAt = cs.EnrolledAt,
            ClassRole = cs.ClassRole
        });
    }

    public async Task<StudentInClassDto> AddStudentToClassAsync(string classId, AddStudentRequestDto request)
    {
        // 1. Kiểm tra Lớp học
        var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);
        if (classEntity == null)
            throw new KeyNotFoundException("Không tìm thấy lớp học.");

        if (classEntity.EndDate.HasValue && classEntity.EndDate.Value < DateOnly.FromDateTime(DateTime.Now))
            throw new InvalidOperationException("Lớp học này đã kết thúc. Không thể thêm học viên mới.");

        // 2. Kiểm tra Sinh viên có tồn tại và phải mang Role là "student"
        var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.StudentId);
        if (student == null)
            throw new InvalidOperationException("Không tìm thấy tài khoản học viên này trong hệ thống.");

        if (student.Role != "student")
            throw new InvalidOperationException($"Tài khoản '{request.StudentId}' không phải là Học viên.");

        // 3. Kiểm tra trùng lặp: Sinh viên này đã ở trong lớp chưa?
        bool isAlreadyEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == request.StudentId);

        if (isAlreadyEnrolled)
            throw new InvalidOperationException("Học viên này đã có tên trong danh sách lớp.");

        // 4. Lưu vào Database
        var newEnrollment = new ClassStudent
        {
            ClassId = classId.ToUpper(),
            StudentId = request.StudentId
            // Ngày EnrolledAt sẽ do SQL Server tự sinh nhờ cấu hình DEFAULT SYSDATETIME()
        };

        _context.ClassStudents.Add(newEnrollment);
        await _context.SaveChangesAsync();

        return new StudentInClassDto
        {
            StudentId = student.Id,
            FullName = student.FullName,
            Email = student.Email,
            AvatarUrl = student.AvatarUrl,
            EnrolledAt = newEnrollment.EnrolledAt // Hoặc dùng DateTime.Now nếu DB chưa kịp trả về
        };
    }

    public async Task<bool> RemoveStudentFromClassAsync(string classId, string studentId)
    {
        var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);
        if (classEntity == null)
            throw new KeyNotFoundException("Không tìm thấy lớp học.");

        if (classEntity.EndDate.HasValue && classEntity.EndDate.Value < DateOnly.FromDateTime(DateTime.Now))
            throw new InvalidOperationException("Lớp học này đã kết thúc. Không thể xóa học viên khỏi lớp.");

        var enrollment = await _context.ClassStudents
            .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);

        if (enrollment == null)
            throw new KeyNotFoundException("Học viên này không tồn tại trong lớp học.");

        _context.ClassStudents.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }
}