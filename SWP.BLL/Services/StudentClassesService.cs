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

    // ─── Lấy danh sách lớp học ────────────────────────────────────────────────

    public async Task<IEnumerable<ClassResponseDto>> GetClassesForStudentAsync(string studentId, Guid? academicTermId = null)
    {
        var query = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.AcademicTerm)
            .Include(c => c.Lecturer)
            .Include(c => c.ClassStudents)
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

    // ─── Lấy danh sách buổi học (lộ trình theo tuần) ───────────────────────────

    public async Task<IEnumerable<ClassSessionDto>> GetClassSessionsAsync(string classId)
    {
        // Lấy tất cả buổi học của lớp, sắp xếp theo ngày
        var sessions = await _context.ClassSessions
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        if (!sessions.Any())
            return Enumerable.Empty<ClassSessionDto>();

        // Tính WeekNumber: buổi đầu tiên = tuần 1
        var firstDate = sessions.First().SessionDate.ToDateTime(TimeOnly.MinValue);

        return sessions.Select(s =>
        {
            var sessionDate = s.SessionDate.ToDateTime(TimeOnly.MinValue);
            var daysDiff = (sessionDate - firstDate).TotalDays;
            var weekNumber = (int)Math.Floor(daysDiff / 7) + 1;

            return new ClassSessionDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                WeekNumber = weekNumber,
                Title = s.Title,
                SessionDate = s.SessionDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Description = s.Detail,
                Room = s.Room
            };
        });
    }

    // ─── Lộ trình học tập (Roadmap) ───────────────────────────────────────────

    public async Task<StudentClassRoadmapDto> GetClassRoadmapAsync(string studentId, string classId)
    {
        // Kiểm tra lớp tồn tại
        var classEntity = await _context.Classes
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == classId);

        if (classEntity == null)
            throw new KeyNotFoundException("Không tìm thấy lớp học.");

        // Lấy toàn bộ học liệu đang active của lớp, kèm thông tin hoàn thành
        var materials = await _context.LearningMaterials
            .Include(m => m.MaterialCompletions)
            .Where(m => m.ClassId == classId && m.IsDisabled == false)
            .ToListAsync();

        // Map và nhóm theo ChapterName
        var mappedMaterials = materials.Select(m =>
        {
            var completion = m.MaterialCompletions
                .FirstOrDefault(c => c.StudentId == studentId);

            return new
            {
                ChapterName = string.IsNullOrWhiteSpace(m.Chapter) ? "Chung" : m.Chapter.Trim(),
                Dto = new StudentRoadmapMaterialDto
                {
                    Id         = m.Id,
                    ClassId    = m.ClassId,
                    Title      = m.Title,
                    Description = m.Description,
                    Type       = m.MaterialType,
                    FileUrl    = m.FileUrl,
                    FileSize   = m.FileSize,
                    UploadedAt = m.UploadedAt,
                    IsCompleted = completion != null,
                    CompletedAt = completion?.CompletedAt
                }
            };
        }).ToList();

        var chapters = mappedMaterials
            .GroupBy(m => m.ChapterName)
            .Select(g => new ChapterRoadmapDto
            {
                ChapterName = g.Key,
                Materials   = g.Select(x => x.Dto)
                               .OrderBy(m => m.UploadedAt)
                               .ToList()
            })
            .ToList();

        return new StudentClassRoadmapDto
        {
            ClassId   = classId,
            ClassName = classEntity.Name ?? classEntity.Course?.Name ?? "Lớp học",
            Chapters  = chapters
        };
    }

    // ─── Đánh dấu hoàn thành ──────────────────────────────────────────────────

    public async Task<bool> CompleteMaterialAsync(string studentId, Guid materialId)
    {
        var material = await _context.LearningMaterials
            .FirstOrDefaultAsync(m => m.Id == materialId && m.IsDisabled == false);

        if (material == null)
            throw new KeyNotFoundException("Không tìm thấy học liệu.");

        // Nếu đã đánh dấu rồi thì bỏ qua
        var existing = await _context.MaterialCompletions
            .FirstOrDefaultAsync(mc => mc.MaterialId == materialId && mc.StudentId == studentId);

        if (existing != null)
            return true;

        _context.MaterialCompletions.Add(new MaterialCompletion
        {
            MaterialId  = materialId,
            StudentId   = studentId,
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    // ─── Hủy đánh dấu hoàn thành ─────────────────────────────────────────────

    public async Task<bool> UncompleteMaterialAsync(string studentId, Guid materialId)
    {
        var existing = await _context.MaterialCompletions
            .FirstOrDefaultAsync(mc => mc.MaterialId == materialId && mc.StudentId == studentId);

        if (existing == null)
            return true; // Đã uncomplete rồi

        _context.MaterialCompletions.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    // ─── Helper mapping ───────────────────────────────────────────────────────

    private static ClassResponseDto MapToDto(Class classEntity) => new()
    {
        Id           = classEntity.Id,
        CourseId     = classEntity.CourseId,
        CourseCode   = classEntity.Course?.Code ?? "N/A",
        CourseName   = classEntity.Course?.Name ?? "N/A",
        AcademicTermId = classEntity.AcademicTermId,
        TermCode     = classEntity.AcademicTerm?.TermCode ?? "N/A",
        LecturerId   = classEntity.LecturerId,
        LecturerName = classEntity.Lecturer?.FullName ?? "N/A",
        StartDate    = classEntity.StartDate ?? classEntity.AcademicTerm?.StartDate,
        EndDate      = classEntity.EndDate   ?? classEntity.AcademicTerm?.EndDate,
        TotalStudents = classEntity.ClassStudents?.Count ?? 0,
        AllowReviewAfterEnd = classEntity.AllowReviewAfterEnd,
        CreatedAt    = classEntity.CreatedAt
    };
}
