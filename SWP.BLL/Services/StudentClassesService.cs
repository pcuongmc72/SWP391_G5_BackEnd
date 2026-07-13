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

    public async Task<IEnumerable<SWP.BLL.DTOs.Classes.ClassSessionDto>> GetClassSessionsAsync(string classId)
    {
        // Lấy tất cả buổi học của lớp, sắp xếp theo ngày
        var sessions = await _context.ClassSessions
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        if (!sessions.Any())
            return Enumerable.Empty<SWP.BLL.DTOs.Classes.ClassSessionDto>();

        // Tính WeekNumber: buổi đầu tiên = tuần 1
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

    // ─── Danh sách bài tập (student view) ───────────────────────────────────────

    public async Task<IEnumerable<StudentAssignmentDto>> GetStudentAssignmentsAsync(
        string studentId, string classId)
    {
        // Xác thực sinh viên có trong lớp
        var isEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (!isEnrolled)
            throw new UnauthorizedAccessException("Bạn không phải thành viên của lớp học này.");

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
                Id          = a.Id,
                ClassId     = a.ClassId,
                Title       = a.Title,
                Description = a.Description,
                DueDate     = a.DueDate,
                MaxPoints   = a.MaxPoints,
                MySubmission = sub == null ? null : MapSubmission(sub)
            };
        });
    }

    // ─── Nộp bài tập ─────────────────────────────────────────────────────────

    public async Task<SubmissionDto> SubmitAssignmentAsync(
        string studentId, string classId, Guid assignmentId, SubmitAssignmentRequestDto request)
    {
        // Xác thực sinh viên có trong lớp
        var isEnrolled = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (!isEnrolled)
            throw new UnauthorizedAccessException("Bạn không phải thành viên của lớp học này.");

        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ClassId == classId);

        if (assignment == null)
            throw new KeyNotFoundException("Không tìm thấy bài tập.");

        // Kiểm tra deadline (cho phép nộp muộn, đánh dấu LATE)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isLate = today > assignment.DueDate;

        // Tìm bài nộp cũ (nếu có)
        var existing = await _context.Submissions
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        if (existing != null)
        {
            // Không cho nộp lại khi đã chấm điểm
            if (existing.Status == "GRADED")
                throw new InvalidOperationException("Bài tập đã được chấm điểm, không thể nộp lại.");

            // Cập nhật bài nộp cũ
            existing.FileName     = request.FileName;
            existing.StudentNotes = request.StudentNotes;
            existing.SubmittedAt  = DateTime.UtcNow;
            existing.Status       = isLate ? "LATE" : "SUBMITTED";
            await _context.SaveChangesAsync();
            return MapSubmission(existing);
        }

        // Tạo bài nộp mới
        var submission = new Submission
        {
            AssignmentId  = assignmentId,
            StudentId     = studentId,
            FileName      = request.FileName,
            StudentNotes  = request.StudentNotes,
            Status        = isLate ? "LATE" : "SUBMITTED",
            SubmittedAt   = DateTime.UtcNow
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        // Load navigation property để map
        await _context.Entry(submission).Reference(s => s.Student).LoadAsync();
        return MapSubmission(submission);
    }

    // ─── Shared Submission mapper ───────────────────────────────────────────

    private static SubmissionDto MapSubmission(Submission s) => new()
    {
        Id           = s.Id,
        AssignmentId = s.AssignmentId,
        StudentId    = s.StudentId,
        StudentName  = s.Student?.FullName,
        FileName     = s.FileName,
        StudentNotes = s.StudentNotes,
        Status       = s.Status,
        Grade        = s.Grade,
        Feedback     = s.Feedback,
        SubmittedAt  = s.SubmittedAt.ToString("yyyy-MM-dd HH:mm"),
        GradedAt     = s.GradedAt?.ToString("yyyy-MM-dd HH:mm")
    };

    // ─── Support Feedback ───────────────────────────────────────────────────

    public async Task<IReadOnlyList<FeedbackDto>> GetFeedbacksAsync(string studentId, string classId)
    {
        // 1. Kiểm tra sinh viên có trong lớp không
        var isEnrolled = await _context.ClassStudents.AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (!isEnrolled) throw new KeyNotFoundException("Học viên không thuộc lớp này.");

        var list = await _context.SupportFeedbacks
            .AsNoTracking()
            .Include(f => f.Sender)
            .Include(f => f.AnsweredBy)
            .Where(f => f.ClassId == classId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return list.Select(MapFeedback).ToList();
    }

    public async Task<FeedbackDto> CreateFeedbackAsync(string studentId, string classId, CreateFeedbackDto request)
    {
        var isEnrolled = await _context.ClassStudents.AnyAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (!isEnrolled) throw new KeyNotFoundException("Học viên không thuộc lớp này.");

        var fb = new SupportFeedback
        {
            ClassId = classId,
            SenderId = studentId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Status = "OPEN",
            CreatedAt = DateTime.UtcNow
        };

        _context.SupportFeedbacks.Add(fb);
        await _context.SaveChangesAsync();

        await _context.Entry(fb).Reference(f => f.Sender).LoadAsync();
        return MapFeedback(fb);
    }

    public async Task<FeedbackDto> RespondFeedbackAsAssistantAsync(string studentId, string classId, Guid feedbackId, RespondFeedbackDto request)
    {
        // Kiểm tra sinh viên có vai trò assistant trong lớp không
        var enrollment = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);
        if (enrollment == null) throw new KeyNotFoundException("Học viên không thuộc lớp này.");
        if (enrollment.ClassRole?.ToLower() != "assistant")
        {
            throw new UnauthorizedAccessException("Bạn không có quyền trợ giảng trong lớp này để trả lời câu hỏi.");
        }

        var fb = await _context.SupportFeedbacks
            .Include(f => f.Sender)
            .Include(f => f.AnsweredBy)
            .FirstOrDefaultAsync(f => f.Id == feedbackId && f.ClassId == classId);

        if (fb == null) throw new KeyNotFoundException("Không tìm thấy phản hồi.");

        fb.Status = "RESPONDED";
        fb.Response = request.Response.Trim();
        fb.RespondedAt = DateTime.UtcNow;
        fb.AnsweredByUserId = studentId;

        await _context.SaveChangesAsync();
        await _context.Entry(fb).Reference(f => f.AnsweredBy).LoadAsync();
        return MapFeedback(fb);
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
        AnsweredByUserId = f.AnsweredByUserId,
        AnsweredByName = f.AnsweredBy?.FullName
    };
}
