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

    /// <summary>
    /// Lấy danh sách lớp học của sinh viên hiện tại.
    /// Filter tùy chọn theo academicTermId hoặc year (cột TermCode, VD: "2024-2025").
    /// </summary>
    public async Task<IEnumerable<MyClassResponseDto>> GetMyClassesAsync(
        string studentId,
        Guid? academicTermId = null,
        string? year = null)
    {
        var query = _context.ClassStudents
            .Where(cs => cs.StudentId == studentId)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.Course)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.AcademicTerm)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.Lecturer)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.ClassStudents)
            .AsQueryable();

        // Filter theo kỳ học cụ thể
        if (academicTermId.HasValue)
            query = query.Where(cs => cs.Class.AcademicTermId == academicTermId.Value);

        // Filter theo năm học (dựa vào TermCode chứa chuỗi năm, VD: "HK2-2024-2025")
        if (!string.IsNullOrWhiteSpace(year))
            query = query.Where(cs => cs.Class.AcademicTerm.TermCode.Contains(year));

        var enrollments = await query
            .OrderByDescending(cs => cs.EnrolledAt)
            .ToListAsync();

        var classIds = enrollments.Select(e => e.ClassId).ToList();

        // Lấy danh sách số lượng tài liệu của mỗi lớp học viên tham gia
        var materialsCountDict = await _context.Materials
            .Where(m => classIds.Contains(m.ClassId))
            .GroupBy(m => m.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count);

        // Lấy danh sách số lượng tài liệu đã hoàn tất của mỗi lớp
        var completedCountDict = await _context.MaterialCompletions
            .Where(mc => mc.StudentId == studentId && classIds.Contains(mc.Material.ClassId))
            .GroupBy(mc => mc.Material.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count);

        return enrollments.Select(cs =>
        {
            var total = materialsCountDict.TryGetValue(cs.ClassId, out var t) ? t : 0;
            var completed = completedCountDict.TryGetValue(cs.ClassId, out var c) ? c : 0;
            var progress = total > 0 ? (int)Math.Round((double)completed / total * 100) : 0;

            return new MyClassResponseDto
            {
                Id               = cs.Class.Id,
                CourseId         = cs.Class.CourseId,
                CourseCode       = cs.Class.Course?.Code ?? "N/A",
                CourseName       = cs.Class.Course?.Name ?? "N/A",
                AcademicTermId   = cs.Class.AcademicTermId,
                TermCode         = cs.Class.AcademicTerm?.TermCode ?? "N/A",
                TermName         = cs.Class.AcademicTerm?.Name ?? "N/A",
                TermStartDate    = cs.Class.AcademicTerm?.StartDate,
                TermEndDate      = cs.Class.AcademicTerm?.EndDate,
                LecturerId       = cs.Class.LecturerId,
                LecturerName     = cs.Class.Lecturer?.FullName ?? "Chưa phân công",
                LecturerEmail    = cs.Class.Lecturer?.Email,
                StartDate        = cs.Class.StartDate,
                EndDate          = cs.Class.EndDate,
                AllowReviewAfterEnd = cs.Class.AllowReviewAfterEnd,
                TotalStudents    = cs.Class.ClassStudents?.Count ?? 0,
                EnrolledAt       = cs.EnrolledAt,
                MaterialProgress = progress
            };
        });
    }

    public async Task<IEnumerable<MyClassResponseDto>> GetMyClassesGradesAsync(string studentId)
    {
        var query = _context.ClassStudents
            .Where(cs => cs.StudentId == studentId)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.Course)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.AcademicTerm)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.Lecturer)
            .Include(cs => cs.Class)
                .ThenInclude(c => c.ClassStudents)
            .AsQueryable();

        var enrollments = await query
            .OrderByDescending(cs => cs.EnrolledAt)
            .ToListAsync();

        var classIds = enrollments.Select(e => e.ClassId).ToList();

        // Lấy danh sách số lượng tài liệu của mỗi lớp học viên tham gia
        var materialsCountDict = await _context.Materials
            .Where(m => classIds.Contains(m.ClassId))
            .GroupBy(m => m.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count);

        // Lấy danh sách số lượng tài liệu đã hoàn tất của mỗi lớp
        var completedCountDict = await _context.MaterialCompletions
            .Where(mc => mc.StudentId == studentId && classIds.Contains(mc.Material.ClassId))
            .GroupBy(mc => mc.Material.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count);

        // Lấy danh sách bài nộp của sinh viên này cho các lớp học
        var submissions = await _context.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == studentId && classIds.Contains(s.Assignment.ClassId))
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);

        return enrollments.Select(cs =>
        {
            var total = materialsCountDict.TryGetValue(cs.ClassId, out var t) ? t : 0;
            var completed = completedCountDict.TryGetValue(cs.ClassId, out var c) ? c : 0;
            var progress = total > 0 ? (int)Math.Round((double)completed / total * 100) : 0;

            // Tính toán GPA & số bài đã chấm
            var classSubmissions = submissions.Where(s => s.Assignment.ClassId == cs.ClassId).ToList();
            var gradedSubmissions = classSubmissions.Where(s => s.Grade.HasValue).ToList();
            int gradedCount = gradedSubmissions.Count;
            decimal? averageGrade = null;
            if (gradedCount > 0)
            {
                averageGrade = gradedSubmissions.Average(s => s.Grade.Value);
            }

            // Trạng thái học tập: chỉ xác định khi có EndDate hoặc AcademicTerm.EndDate
            var endDate = cs.Class.EndDate ?? cs.Class.AcademicTerm?.EndDate;
            string? learningStatus = null;
            if (endDate.HasValue)
            {
                learningStatus = endDate.Value < today ? "Hoàn thành" : "Đang học";
            }

            // Trích xuất năm học từ TermCode (ví dụ: "HK2-2024-2025" -> "2024-2025")
            string academicYear = "N/A";
            var termCode = cs.Class.AcademicTerm?.TermCode ?? "";
            var match = System.Text.RegularExpressions.Regex.Match(termCode, @"\d{4}-\d{4}");
            if (match.Success)
            {
                academicYear = match.Value;
            }
            else if (cs.Class.AcademicTerm?.StartDate != null)
            {
                var startYr = cs.Class.AcademicTerm.StartDate.Year;
                academicYear = $"{startYr}-{startYr + 1}";
            }

            return new MyClassResponseDto
            {
                Id               = cs.Class.Id,
                CourseId         = cs.Class.CourseId,
                CourseCode       = cs.Class.Course?.Code ?? "N/A",
                CourseName       = cs.Class.Course?.Name ?? "N/A",
                AcademicTermId   = cs.Class.AcademicTermId,
                TermCode         = cs.Class.AcademicTerm?.TermCode ?? "N/A",
                TermName         = cs.Class.AcademicTerm?.Name ?? "N/A",
                TermStartDate    = cs.Class.AcademicTerm?.StartDate,
                TermEndDate      = cs.Class.AcademicTerm?.EndDate,
                LecturerId       = cs.Class.LecturerId,
                LecturerName     = cs.Class.Lecturer?.FullName ?? "Chưa phân công",
                LecturerEmail    = cs.Class.Lecturer?.Email,
                StartDate        = cs.Class.StartDate,
                EndDate          = cs.Class.EndDate,
                AllowReviewAfterEnd = cs.Class.AllowReviewAfterEnd,
                TotalStudents    = cs.Class.ClassStudents?.Count ?? 0,
                EnrolledAt       = cs.EnrolledAt,
                MaterialProgress = progress,
                GradedAssignmentsCount = gradedCount,
                AverageGrade     = averageGrade,
                LearningStatus   = learningStatus
            };
        });
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