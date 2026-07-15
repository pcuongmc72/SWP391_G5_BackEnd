using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Lecturer;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public partial class LecturerService : ILecturerService
{
    private readonly FlippedClassroomContext _context;

    public LecturerService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LecturerClassListItemDto>> GetMyClassesAsync(string lecturerId)
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.LecturerId == lecturerId)
            .Include(c => c.Course)
            .Include(c => c.AcademicTerm)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new LecturerClassListItemDto
            {
                Id = c.Id,
                Name = c.Name ?? c.Id,
                CourseCode = c.Course.Code,
                CourseName = c.Course.Name,
                TermName = c.AcademicTerm.Name,
                StudentCount = c.ClassStudents.Count,
                SessionCount = c.ClassSessions.Count
            })
            .ToListAsync();
    }

    public async Task<LecturerClassDetailDto> GetClassDetailAsync(string lecturerId, string classId)
    {
        var cls = await _context.Classes
            .AsNoTracking()
            .Include(c => c.Course)
            .Include(c => c.AcademicTerm)
            .FirstOrDefaultAsync(c => c.Id == classId && c.LecturerId == lecturerId);

        if (cls is null)
            throw new KeyNotFoundException("Khong tim thay lop hoc hoac ban khong co quyen truy cap.");

        var studentCount = await _context.ClassStudents.CountAsync(cs => cs.ClassId == classId);

        return new LecturerClassDetailDto
        {
            Id = cls.Id,
            Name = cls.Name ?? cls.Id,
            AllowReviewAfterEnd = cls.AllowReviewAfterEnd,
            CourseId = cls.CourseId,
            CourseCode = cls.Course.Code,
            CourseName = cls.Course.Name,
            CourseDescription = cls.Course.Description,
            AcademicTermId = cls.AcademicTermId,
            TermName = cls.AcademicTerm.Name,
            TermStartDate = cls.AcademicTerm.StartDate,
            TermEndDate = cls.AcademicTerm.EndDate,
            StudentCount = studentCount
        };
    }

    public async Task<IReadOnlyList<ClassSessionDto>> GetSessionsAsync(
        string lecturerId, string classId, DateOnly? from, DateOnly? to)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var query = _context.ClassSessions
            .AsNoTracking()
            .Where(s => s.ClassId == classId);

        if (from.HasValue)
            query = query.Where(s => s.SessionDate >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.SessionDate <= to.Value);

        var sessions = await query
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(MapSession).ToList();
    }

    public async Task<ClassSessionDto> GetSessionAsync(string lecturerId, string classId, Guid sessionId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var session = await _context.ClassSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.ClassId == classId);

        if (session is null)
            throw new KeyNotFoundException("Khong tim thay buoi hoc.");

        return MapSession(session);
    }

    public async Task<ClassSessionDto> CreateSessionAsync(
        string lecturerId, string classId, UpsertClassSessionDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);
        ValidateSessionRequest(request);

        var now = DateTime.UtcNow;
        var session = new ClassSession
        {
            ClassId = classId,
            SessionDate = request.SessionDate,
            StartTime = ParseTime(request.StartTime),
            EndTime = ParseTime(request.EndTime),
            Title = request.Title.Trim(),
            Detail = string.IsNullOrWhiteSpace(request.Detail) ? null : request.Detail.Trim(),
            Room = string.IsNullOrWhiteSpace(request.Room) ? null : request.Room.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.ClassSessions.Add(session);
        await _context.SaveChangesAsync();

        return MapSession(session);
    }

    public async Task<ClassSessionDto> UpdateSessionAsync(
        string lecturerId, string classId, Guid sessionId, UpsertClassSessionDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);
        ValidateSessionRequest(request);

        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.ClassId == classId);

        if (session is null)
            throw new KeyNotFoundException("Khong tim thay buoi hoc.");

        session.SessionDate = request.SessionDate;
        session.StartTime = ParseTime(request.StartTime);
        session.EndTime = ParseTime(request.EndTime);
        session.Title = request.Title.Trim();
        session.Detail = string.IsNullOrWhiteSpace(request.Detail) ? null : request.Detail.Trim();
        session.Room = string.IsNullOrWhiteSpace(request.Room) ? null : request.Room.Trim();
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapSession(session);
    }

    public async Task DeleteSessionAsync(string lecturerId, string classId, Guid sessionId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var session = await _context.ClassSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.ClassId == classId);

        if (session is null)
            throw new KeyNotFoundException("Khong tim thay buoi hoc.");

        _context.ClassSessions.Remove(session);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureClassAccessAsync(string lecturerId, string classId)
    {
        var isLecturer = await _context.Classes
            .AnyAsync(c => c.Id == classId && c.LecturerId == lecturerId);

        if (isLecturer) return;

        var isAssistant = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == lecturerId && cs.ClassRole == "assistant");

        if (!isAssistant)
            throw new KeyNotFoundException("Khong tim thay lop hoc hoac ban khong co quyen truy cap.");
    }

    private static void ValidateSessionRequest(UpsertClassSessionDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Tieu de buoi hoc khong duoc de trong.");
    }

    private static TimeOnly? ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (TimeOnly.TryParse(value, out var time))
            return time;

        throw new ArgumentException($"Gio khong hop le: {value}");
    }

    private static ClassSessionDto MapSession(ClassSession session) => new()
    {
        Id = session.Id,
        ClassId = session.ClassId,
        SessionDate = session.SessionDate,
        StartTime = session.StartTime?.ToString("HH:mm"),
        EndTime = session.EndTime?.ToString("HH:mm"),
        Title = session.Title,
        Detail = session.Detail,
        Room = session.Room
    };
}
