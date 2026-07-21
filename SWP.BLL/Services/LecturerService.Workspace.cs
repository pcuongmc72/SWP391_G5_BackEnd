using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Lecturer;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public partial class LecturerService
{

    public async Task<IReadOnlyList<ClassStudentDto>> GetClassStudentsAsync(string lecturerId, string classId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        return await _context.ClassStudents
            .AsNoTracking()
            .Where(cs => cs.ClassId == classId)
            .Select(cs => new ClassStudentDto
            {
                Id = cs.Student.Id,
                FullName = cs.Student.FullName,
                Email = cs.Student.Email,
                AvatarUrl = cs.Student.AvatarUrl,
                ClassRole = cs.ClassRole ?? "student"
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MaterialDto>> GetMaterialsAsync(string lecturerId, string classId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var list = await _context.LearningMaterials
            .AsNoTracking()
            .Include(m => m.MaterialCompletions)
            .Where(m => m.ClassId == classId && !m.IsDisabled)

            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();

        return list.Select(MapMaterial).ToList();
    }

    public async Task<MaterialDto> CreateMaterialAsync(string lecturerId, string classId, UpsertMaterialDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);
        ValidateMaterialType(request.Type);

        var entity = new LearningMaterial
        {
            ClassId = classId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Chapter = string.IsNullOrWhiteSpace(request.Chapter) ? null : request.Chapter.Trim(),
            MaterialType = request.Type.ToLower(),
            FileUrl = request.FileUrl,
            FileSize = request.FileSize,
            UploadedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };

        _context.LearningMaterials.Add(entity);
        await _context.SaveChangesAsync();
        return MapMaterial(entity);
    }

    public async Task<MaterialDto> UpdateMaterialAsync(string lecturerId, string classId, Guid materialId, UpsertMaterialDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);
        ValidateMaterialType(request.Type);

        var entity = await _context.LearningMaterials
            .Include(m => m.MaterialCompletions)
            .FirstOrDefaultAsync(m => m.Id == materialId && m.ClassId == classId);

        if (entity is null) throw new KeyNotFoundException("Không tìm thấy học liệu.");

        entity.Title = request.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.Chapter = string.IsNullOrWhiteSpace(request.Chapter) ? null : request.Chapter.Trim();
        entity.MaterialType = request.Type.ToLower();
        entity.FileUrl = request.FileUrl;
        entity.FileSize = request.FileSize;

        await _context.SaveChangesAsync();
        return MapMaterial(entity);
    }

    public async Task DeleteMaterialAsync(string lecturerId, string classId, Guid materialId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var entity = await _context.LearningMaterials
            .FirstOrDefaultAsync(m => m.Id == materialId && m.ClassId == classId);

        if (entity is null) throw new KeyNotFoundException("Không tìm thấy học liệu.");

        // Soft delete – disable the material but keep it in DB so lecturer can still see it
        entity.IsDisabled = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkMaterialCompleteAsync(string lecturerId, string classId, Guid materialId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var material = await _context.LearningMaterials
            .FirstOrDefaultAsync(m => m.Id == materialId && m.ClassId == classId);

        if (material is null) throw new KeyNotFoundException("Không tìm thấy học liệu.");

        var studentIds = await _context.ClassStudents
            .Where(cs => cs.ClassId == classId)
            .Select(cs => cs.StudentId)
            .ToListAsync();

        var existing = await _context.MaterialCompletions
            .Where(mc => mc.MaterialId == materialId)
            .ToListAsync();

        _context.MaterialCompletions.RemoveRange(existing);

        foreach (var sid in studentIds)
        {
            _context.MaterialCompletions.Add(new MaterialCompletion
            {
                MaterialId = materialId,
                StudentId = sid,
                CompletedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AssignmentDto>> GetAssignmentsAsync(string lecturerId, string classId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        return await _context.Assignments
            .AsNoTracking()
            .Where(a => a.ClassId == classId)
            .OrderByDescending(a => a.DueDate)
            .Select(a => new AssignmentDto
            {
                Id = a.Id,
                ClassId = a.ClassId,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                MaxPoints = a.MaxPoints,
                SubmissionCount = a.Submissions.Count
            })
            .ToListAsync();
    }

    public async Task<AssignmentDto> CreateAssignmentAsync(string lecturerId, string classId, UpsertAssignmentDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var entity = new Assignment
        {
            ClassId = classId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DueDate = request.DueDate,
            MaxPoints = request.MaxPoints,
            CreatedAt = DateTime.UtcNow
        };

        _context.Assignments.Add(entity);
        await _context.SaveChangesAsync();

        return new AssignmentDto
        {
            Id = entity.Id,
            ClassId = entity.ClassId,
            Title = entity.Title,
            Description = entity.Description,
            DueDate = entity.DueDate,
            MaxPoints = entity.MaxPoints,
            SubmissionCount = 0
        };
    }

    public async Task<AssignmentDto> UpdateAssignmentAsync(string lecturerId, string classId, Guid assignmentId, UpsertAssignmentDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var entity = await _context.Assignments
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ClassId == classId);

        if (entity is null) throw new KeyNotFoundException("Không tìm thấy bài tập.");

        entity.Title = request.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.DueDate = request.DueDate;
        entity.MaxPoints = request.MaxPoints;

        await _context.SaveChangesAsync();

        return new AssignmentDto
        {
            Id = entity.Id,
            ClassId = entity.ClassId,
            Title = entity.Title,
            Description = entity.Description,
            DueDate = entity.DueDate,
            MaxPoints = entity.MaxPoints,
            SubmissionCount = entity.Submissions.Count
        };
    }

    public async Task DeleteAssignmentAsync(string lecturerId, string classId, Guid assignmentId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var entity = await _context.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.ClassId == classId);

        if (entity is null) throw new KeyNotFoundException("Không tìm thấy bài tập.");

        _context.Assignments.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<SubmissionDto>> GetSubmissionsAsync(string lecturerId, string classId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var list = await _context.Submissions
            .AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .Where(s => s.Assignment.ClassId == classId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        return list.Select(MapSubmission).ToList();
    }

    public async Task<SubmissionDto> GradeSubmissionAsync(
        string lecturerId, string classId, Guid submissionId, GradeSubmissionDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var submission = await _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.Assignment.ClassId == classId);

        if (submission is null) throw new KeyNotFoundException("Không tìm thấy bài nộp.");

        submission.Status = "GRADED";
        submission.Grade = request.Grade;
        submission.Feedback = request.Feedback.Trim();
        submission.GradedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapSubmission(submission);
    }

    public async Task<IReadOnlyList<FeedbackDto>> GetFeedbacksAsync(string lecturerId, string classId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var list = await _context.SupportFeedbacks
            .AsNoTracking()
            .Include(f => f.Sender)
            .Include(f => f.Material)   // <-- Include tên bài học
            .Where(f => f.ClassId == null || f.ClassId == classId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return list.Select(MapFeedback).ToList();
    }

    public async Task<FeedbackDto> RespondFeedbackAsync(
        string lecturerId, string classId, Guid feedbackId, RespondFeedbackDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var fb = await _context.SupportFeedbacks
            .Include(f => f.Sender)
            .Include(f => f.Material)   // <-- Include tên bài học
            .FirstOrDefaultAsync(f => f.Id == feedbackId && (f.ClassId == null || f.ClassId == classId));

        if (fb is null) throw new KeyNotFoundException("Không tìm thấy phản hồi.");

        fb.Status = "RESPONDED";
        fb.Response = request.Response;
        fb.RespondedAt = DateTime.UtcNow;
        fb.AnsweredById = lecturerId;
        fb.AnsweredByName = (await _context.Users.FindAsync(lecturerId))?.FullName ?? "Giảng viên";
        fb.AnsweredByRole = "lecturer";

        _context.SupportFeedbacks.Update(fb);
        await _context.SaveChangesAsync();
        return MapFeedback(fb);
    }

    public async Task<IReadOnlyList<DiscussionThreadDto>> GetThreadsAsync(string lecturerId, string classId)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var threads = await _context.DiscussionThreads
            .AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.DiscussionReplies)
                .ThenInclude(r => r.Author)
            .Where(t => t.ClassId == null || t.ClassId == classId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return threads.Select(MapThread).ToList();
    }

    public async Task<DiscussionThreadDto> CreateThreadAsync(string lecturerId, string classId, UpsertThreadDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var entity = new DiscussionThread
        {
            ClassId = classId,
            AuthorId = lecturerId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.DiscussionThreads.Add(entity);
        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(t => t.Author).LoadAsync();
        return MapThread(entity);
    }

    public async Task<DiscussionReplyDto> CreateReplyAsync(
        string lecturerId, string classId, Guid threadId, CreateReplyDto request)
    {
        await EnsureClassAccessAsync(lecturerId, classId);

        var thread = await _context.DiscussionThreads
            .FirstOrDefaultAsync(t => t.Id == threadId && (t.ClassId == null || t.ClassId == classId));

        if (thread is null) throw new KeyNotFoundException("Không tìm thấy chủ đề.");

        var reply = new DiscussionReply
        {
            ThreadId = threadId,
            AuthorId = lecturerId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.DiscussionReplies.Add(reply);
        await _context.SaveChangesAsync();

        await _context.Entry(reply).Reference(r => r.Author).LoadAsync();
        return MapReply(reply);
    }

    private static MaterialDto MapMaterial(LearningMaterial m) => new()
    {
        Id = m.Id,
        ClassId = m.ClassId,
        Title = m.Title,
        Description = m.Description,
        Chapter = m.Chapter,
        Type = m.MaterialType,
        FileUrl = m.FileUrl,
        FileSize = m.FileSize,
        UploadedAt = m.UploadedAt,
        CompletedByUsers = m.MaterialCompletions?.Select(c => c.StudentId).ToList() ?? new List<string>(),
        IsDisabled = m.IsDisabled
    };

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

    private static DiscussionThreadDto MapThread(DiscussionThread t) => new()
    {
        Id = t.Id,
        ClassId = t.ClassId,
        AuthorId = t.AuthorId,
        AuthorName = t.Author?.FullName,
        Title = t.Title,
        Content = t.Content,
        CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd"),
        Replies = t.DiscussionReplies?
            .OrderBy(r => r.CreatedAt)
            .Select(MapReply)
            .ToList() ?? new List<DiscussionReplyDto>()
    };

    private static DiscussionReplyDto MapReply(DiscussionReply r) => new()
    {
        Id = r.Id,
        AuthorId = r.AuthorId,
        AuthorName = r.Author?.FullName,
        Content = r.Content,
        CreatedAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm")
    };

    private static void ValidateMaterialType(string type)
    {
        var valid = new[] { "video", "pdf", "document", "quiz", "image", "link" };
        if (!valid.Contains(type.ToLower()))
            throw new ArgumentException("Loại học liệu không hợp lệ.");
    }

    public async Task PromoteStudentAsync(string lecturerId, string classId, string studentId, string role)
    {
        var isLecturer = await _context.Classes.AnyAsync(c => c.Id == classId && c.LecturerId == lecturerId);
        if (!isLecturer)
            throw new UnauthorizedAccessException("Chỉ giảng viên chính mới có thể thay đổi vai trò.");

        var validRoles = new[] { "student", "assistant" };
        if (!validRoles.Contains(role.ToLower()))
            throw new ArgumentException($"Vai trò không hợp lệ: {role}. Chỉ chấp nhận: student, assistant.");

        var enrollment = await _context.ClassStudents
            .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);

        if (enrollment is null)
            throw new KeyNotFoundException("Học sinh không thuộc lớp học này.");

        enrollment.ClassRole = role.ToLower();
        await _context.SaveChangesAsync();
    }
}
