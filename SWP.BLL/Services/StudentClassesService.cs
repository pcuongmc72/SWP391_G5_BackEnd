using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Classes;
using SWP.BLL.DTOs.Lecturer;
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

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ LĂ¡ÂºÂ¥y danh sÄ‚Â¡ch lĂ¡Â»â€ºp hĂ¡Â»Âc Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

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

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ LĂ¡ÂºÂ¥y danh sÄ‚Â¡ch buĂ¡Â»â€¢i hĂ¡Â»Âc (lĂ¡Â»â„¢ trÄ‚Â¬nh theo tuĂ¡ÂºÂ§n) Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<IEnumerable<SWP.BLL.DTOs.Classes.ClassSessionDto>> GetClassSessionsAsync(string classId)
    {
        // LĂ¡ÂºÂ¥y tĂ¡ÂºÂ¥t cĂ¡ÂºÂ£ buĂ¡Â»â€¢i hĂ¡Â»Âc cĂ¡Â»Â§a lĂ¡Â»â€ºp, sĂ¡ÂºÂ¯p xĂ¡ÂºÂ¿p theo ngÄ‚Â y
        var sessions = await _context.ClassSessions
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        if (!sessions.Any())
            return Enumerable.Empty<SWP.BLL.DTOs.Classes.ClassSessionDto>();

        // TÄ‚Â­nh WeekNumber: buĂ¡Â»â€¢i Ă„â€˜Ă¡ÂºÂ§u tiÄ‚Âªn = tuĂ¡ÂºÂ§n 1
        var firstDate = sessions.First().SessionDate.ToDateTime(TimeOnly.MinValue);

        return sessions.Select(s =>
        {
            var sessionDate = s.SessionDate.ToDateTime(TimeOnly.MinValue);
            var daysDiff = (sessionDate - firstDate).TotalDays;
            var weekNumber = (int)Math.Floor(daysDiff / 7) + 1;

            return new SWP.BLL.DTOs.Classes.ClassSessionDto
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

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ LĂ¡Â»â„¢ trÄ‚Â¬nh hĂ¡Â»Âc tĂ¡ÂºÂ­p (Roadmap) Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<StudentClassRoadmapDto> GetClassRoadmapAsync(string studentId, string classId)
    {
        // KiĂ¡Â»Æ’m tra lĂ¡Â»â€ºp tĂ¡Â»â€œn tĂ¡ÂºÂ¡i
        var classEntity = await _context.Classes
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == classId);

        if (classEntity == null)
            throw new KeyNotFoundException("KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y lĂ¡Â»â€ºp hĂ¡Â»Âc.");

        // LĂ¡ÂºÂ¥y toÄ‚Â n bĂ¡Â»â„¢ hĂ¡Â»Âc liĂ¡Â»â€¡u Ă„â€˜ang active cĂ¡Â»Â§a lĂ¡Â»â€ºp, kÄ‚Â¨m thÄ‚Â´ng tin hoÄ‚Â n thÄ‚Â nh
        var materials = await _context.LearningMaterials
            .Include(m => m.MaterialCompletions)
            .Where(m => m.ClassId == classId && m.IsDisabled == false)
            .ToListAsync();

        // Map vÄ‚Â  nhÄ‚Â³m theo ChapterName
        var mappedMaterials = materials.Select(m =>
        {
            var completion = m.MaterialCompletions
                .FirstOrDefault(c => c.StudentId == studentId);

            return new
            {
                ChapterName = string.IsNullOrWhiteSpace(m.Chapter) ? "Chung" : m.Chapter.Trim(),
                Dto = new StudentRoadmapMaterialDto
                {
                    Id = m.Id,
                    ClassId = m.ClassId,
                    Title = m.Title,
                    Description = m.Description,
                    Type = m.MaterialType,
                    FileUrl = m.FileUrl,
                    FileSize = m.FileSize,
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
                Materials = g.Select(x => x.Dto)
                               .OrderBy(m => m.UploadedAt)
                               .ToList()
            })
            .ToList();

        return new StudentClassRoadmapDto
        {
            ClassId = classId,
            ClassName = classEntity.Name ?? classEntity.Course?.Name ?? "LĂ¡Â»â€ºp hĂ¡Â»Âc",
            Chapters = chapters
        };
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ Ă„ÂÄ‚Â¡nh dĂ¡ÂºÂ¥u hoÄ‚Â n thÄ‚Â nh Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<bool> CompleteMaterialAsync(string studentId, Guid materialId)
    {
        var material = await _context.LearningMaterials
            .FirstOrDefaultAsync(m => m.Id == materialId && m.IsDisabled == false);

        if (material == null)
            throw new KeyNotFoundException("KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y hĂ¡Â»Âc liĂ¡Â»â€¡u.");

        // NĂ¡ÂºÂ¿u Ă„â€˜Ä‚Â£ Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u rĂ¡Â»â€œi thÄ‚Â¬ bĂ¡Â»Â qua
        var existing = await _context.MaterialCompletions
            .FirstOrDefaultAsync(mc => mc.MaterialId == materialId && mc.StudentId == studentId);

        if (existing != null)
            return true;

        _context.MaterialCompletions.Add(new MaterialCompletion
        {
            MaterialId = materialId,
            StudentId = studentId,
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ HĂ¡Â»Â§y Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u hoÄ‚Â n thÄ‚Â nh Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<bool> UncompleteMaterialAsync(string studentId, Guid materialId)
    {
        var existing = await _context.MaterialCompletions
            .FirstOrDefaultAsync(mc => mc.MaterialId == materialId && mc.StudentId == studentId);

        if (existing == null)
            return true; // Ă„ÂÄ‚Â£ uncomplete rĂ¡Â»â€œi

        _context.MaterialCompletions.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ Helper mapping Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

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

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ Danh sÄ‚Â¡ch bÄ‚Â i tĂ¡ÂºÂ­p (student view) Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<IEnumerable<StudentAssignmentDto>> GetStudentAssignmentsAsync(
        string studentId, string classId)
    {
        // XÄ‚Â¡c thĂ¡Â»Â±c sinh viÄ‚Âªn cÄ‚Â³ trong lĂ¡Â»â€ºp
        var isEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (!isEnrolled)
            throw new UnauthorizedAccessException("BĂ¡ÂºÂ¡n khÄ‚Â´ng phĂ¡ÂºÂ£i thÄ‚Â nh viÄ‚Âªn cĂ¡Â»Â§a lĂ¡Â»â€ºp hĂ¡Â»Âc nÄ‚Â y.");

        var assignments = await _context.Assignments
            .AsNoTracking()
            .Include(a => a.Submissions.Where(s => s.StudentId == studentId))
                .ThenInclude(s => s.Student)
            .Where(a => a.ClassId == classId)
            .OrderBy(a => a.DueDate)
            .ToListAsync();

        return assignments.Select(a =>
        {
            var sub = a.Submissions.FirstOrDefault(s => s.StudentId == studentId);
            return new StudentAssignmentDto
            {
                Id = a.Id,
                ClassId = a.ClassId,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                MaxPoints = a.MaxPoints,
                MySubmission = sub == null ? null : MapSubmission(sub)
            };
        });
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ NĂ¡Â»â„¢p bÄ‚Â i tĂ¡ÂºÂ­p Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<SubmissionDto> SubmitAssignmentAsync(
        string studentId, string classId, Guid assignmentId, SubmitAssignmentRequestDto request)
    {
        // XÄ‚Â¡c thĂ¡Â»Â±c sinh viÄ‚Âªn cÄ‚Â³ trong lĂ¡Â»â€ºp
        var isEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (!isEnrolled)
            throw new UnauthorizedAccessException("BĂ¡ÂºÂ¡n khÄ‚Â´ng phĂ¡ÂºÂ£i thÄ‚Â nh viÄ‚Âªn cĂ¡Â»Â§a lĂ¡Â»â€ºp hĂ¡Â»Âc nÄ‚Â y.");

        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ClassId == classId);

        if (assignment == null)
            throw new KeyNotFoundException("KhÄ‚Â´ng tÄ‚Â¬m thĂ¡ÂºÂ¥y bÄ‚Â i tĂ¡ÂºÂ­p.");

        // KiĂ¡Â»Æ’m tra deadline (cho phÄ‚Â©p nĂ¡Â»â„¢p muĂ¡Â»â„¢n, Ă„â€˜Ä‚Â¡nh dĂ¡ÂºÂ¥u LATE)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isLate = today > assignment.DueDate;

        // TÄ‚Â¬m bÄ‚Â i nĂ¡Â»â„¢p cĂ…Â© (nĂ¡ÂºÂ¿u cÄ‚Â³)
        var existing = await _context.Submissions
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        if (existing != null)
        {
            // KhÄ‚Â´ng cho nĂ¡Â»â„¢p lĂ¡ÂºÂ¡i khi Ă„â€˜Ä‚Â£ chĂ¡ÂºÂ¥m Ă„â€˜iĂ¡Â»Æ’m
            if (existing.Status == "GRADED")
                throw new InvalidOperationException("BÄ‚Â i tĂ¡ÂºÂ­p Ă„â€˜Ä‚Â£ Ă„â€˜Ă†Â°Ă¡Â»Â£c chĂ¡ÂºÂ¥m Ă„â€˜iĂ¡Â»Æ’m, khÄ‚Â´ng thĂ¡Â»Æ’ nĂ¡Â»â„¢p lĂ¡ÂºÂ¡i.");

            // CĂ¡ÂºÂ­p nhĂ¡ÂºÂ­t bÄ‚Â i nĂ¡Â»â„¢p cĂ…Â©
            existing.FileName = request.FileName;
            existing.StudentNotes = request.StudentNotes;
            existing.SubmittedAt = DateTime.UtcNow;
            existing.Status = isLate ? "LATE" : "SUBMITTED";
            await _context.SaveChangesAsync();
            return MapSubmission(existing);
        }

        // TĂ¡ÂºÂ¡o bÄ‚Â i nĂ¡Â»â„¢p mĂ¡Â»â€ºi
        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            FileName = request.FileName,
            StudentNotes = request.StudentNotes,
            Status = isLate ? "LATE" : "SUBMITTED",
            SubmittedAt = DateTime.UtcNow
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        // Load navigation property Ă„â€˜Ă¡Â»Æ’ map
        await _context.Entry(submission).Reference(s => s.Student).LoadAsync();
        return MapSubmission(submission);
    }

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ Shared Submission mapper Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    private static SubmissionDto MapSubmission(Submission s) => new()
    {
        Id = s.Id,
        AssignmentId = s.AssignmentId,
        StudentId = s.StudentId,
        StudentName = s.Student?.FullName,
        FileName = s.FileName,
        StudentNotes = s.StudentNotes,
        Status = s.Status,
        Grade = s.Grade,
        Feedback = s.Feedback,
        SubmittedAt = s.SubmittedAt.ToString("yyyy-MM-dd HH:mm"),
        GradedAt = s.GradedAt?.ToString("yyyy-MM-dd HH:mm")
    };

    // Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬ GĂ¡Â»Â­i cÄ‚Â¢u hĂ¡Â»Âi cho GiĂ¡ÂºÂ£ng viÄ‚Âªn Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬Ă¢â€â‚¬

    public async Task<FeedbackDto> CreateFeedbackAsync(string studentId, string classId, CreateFeedbackDto request)
    {
        // XÄ‚Â¡c thĂ¡Â»Â±c sinh viÄ‚Âªn cÄ‚Â³ trong lĂ¡Â»â€ºp
        var isEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (!isEnrolled)
            throw new UnauthorizedAccessException("BĂ¡ÂºÂ¡n khÄ‚Â´ng phĂ¡ÂºÂ£i thÄ‚Â nh viÄ‚Âªn cĂ¡Â»Â§a lĂ¡Â»â€ºp hĂ¡Â»Âc nÄ‚Â y.");

        var entity = new SupportFeedback
        {
            ClassId = classId,
            SenderId = studentId,
            Title = (request.Title ?? "CÄ‚Â¢u hĂ¡Â»Âi").Trim(),
            Message = request.Message.Trim(),
            Status = "OPEN",
            MaterialId = request.MaterialId,
            CreatedAt = DateTime.UtcNow
        };

        _context.SupportFeedbacks.Add(entity);
        await _context.SaveChangesAsync();

        // Load navigation properties
        await _context.Entry(entity).Reference(f => f.Sender).LoadAsync();
        if (entity.MaterialId.HasValue)
            await _context.Entry(entity).Reference(f => f.Material).LoadAsync();

        return MapFeedback(entity);
    }

    public async Task<IReadOnlyList<FeedbackDto>> GetMyFeedbacksAsync(string studentId, string classId)
    {
        var role = await _context.ClassStudents
            .Where(cs => cs.ClassId == classId && cs.StudentId == studentId)
            .Select(cs => cs.ClassRole)
            .FirstOrDefaultAsync();

        bool isAssistant = role == "assistant";

        var query = _context.SupportFeedbacks
            .AsNoTracking()
            .Include(f => f.Sender)
            .Include(f => f.Material)
            .Where(f => f.ClassId == classId);

        if (!isAssistant)
        {
            query = query.Where(f => f.SenderId == studentId);
        }

        var list = await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return list.Select(f => new FeedbackDto
        {
            Id = f.Id,
            ClassId = f.ClassId,
            SenderId = f.SenderId,
            SenderName = f.Sender?.FullName,
            Title = f.Title,
            Message = f.Message,
            Status = f.Status,
            Response = f.Response,
            CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            RespondedAt = f.RespondedAt?.ToString("yyyy-MM-dd HH:mm"),
            MaterialId = f.MaterialId,
            MaterialTitle = f.Material?.Title,
            AnsweredById = f.AnsweredById,
            AnsweredByName = f.AnsweredByName,
            AnsweredByRole = f.AnsweredByRole
        }).ToList();
    }

    public async Task<SWP.BLL.DTOs.Lecturer.FeedbackDto> RespondFeedbackAsAssistantAsync(string assistantId, string classId, Guid feedbackId, SWP.BLL.DTOs.Lecturer.RespondFeedbackDto request)
    {
        // 1. Kiểm tra quyền trợ giảng
        var cs = await _context.ClassStudents
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.ClassId == classId && x.StudentId == assistantId);
        if (cs == null || cs.ClassRole != "assistant")
            throw new UnauthorizedAccessException("Chỉ người hỗ trợ mới có quyền giải đáp.");

        // 2. Tìm feedback
        var fb = await _context.SupportFeedbacks
            .Include(f => f.Sender)
            .Include(f => f.Material)
            .FirstOrDefaultAsync(f => f.Id == feedbackId && f.ClassId == classId);

        if (fb == null)
            throw new KeyNotFoundException("Không tìm thấy câu hỏi.");

        // 3. Cập nhật
        fb.Response = request.Response;
        fb.Status = "RESPONDED";
        fb.RespondedAt = DateTime.UtcNow;
        fb.AnsweredById = assistantId;
        fb.AnsweredByName = cs.Student?.FullName ?? "Trợ giảng";
        fb.AnsweredByRole = "assistant";

        _context.SupportFeedbacks.Update(fb);
        await _context.SaveChangesAsync();

        return new SWP.BLL.DTOs.Lecturer.FeedbackDto
        {
            Id = fb.Id,
            ClassId = fb.ClassId,
            SenderId = fb.SenderId,
            SenderName = fb.Sender.FullName,
            Title = fb.Title,
            Message = fb.Message,
            Status = fb.Status,
            Response = fb.Response,
            CreatedAt = fb.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            RespondedAt = fb.RespondedAt?.ToString("yyyy-MM-dd HH:mm"),
            MaterialId = fb.MaterialId,
            MaterialTitle = fb.Material?.Title,
            AnsweredById = fb.AnsweredById,
            AnsweredByName = fb.AnsweredByName,
            AnsweredByRole = fb.AnsweredByRole
        };
    }

    private static FeedbackDto MapFeedback(SupportFeedback f) => new()
    {
        Id = f.Id,
        ClassId = f.ClassId,
        SenderId = f.SenderId,
        SenderName = f.Sender?.FullName,
        Title = f.Title,
        Message = f.Message,
        Status = f.Status,
        Response = f.Response,
        CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
        RespondedAt = f.RespondedAt?.ToString("yyyy-MM-dd HH:mm"),
        MaterialId = f.MaterialId,
        MaterialTitle = f.Material?.Title,
        AnsweredById = f.AnsweredById,
        AnsweredByName = f.AnsweredByName,
        AnsweredByRole = f.AnsweredByRole
    };
}
