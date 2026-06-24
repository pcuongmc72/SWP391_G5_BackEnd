using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Submissions;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class SubmissionsService : ISubmissionsService
{
    private readonly FlippedClassroomContext _context;

    public SubmissionsService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SubmissionDto>> GetMySubmissionsAsync(string studentId, string? classId = null)
    {
        var query = _context.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == studentId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(classId))
            query = query.Where(s => s.Assignment.ClassId == classId);

        var list = await query.OrderByDescending(s => s.SubmittedAt).ToListAsync();

        return list.Select(MapToDto);
    }

    public async Task<SubmissionDto> SubmitAssignmentAsync(string studentId, SubmitAssignmentRequestDto request)
    {
        // Kiểm tra assignment tồn tại
        var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.Id == request.AssignmentId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài tập.");

        // Kiểm tra sinh viên có trong lớp không
        bool inClass = await _context.ClassStudents
            .AnyAsync(cs => cs.ClassId == assignment.ClassId && cs.StudentId == studentId);
        if (!inClass)
            throw new InvalidOperationException("Bạn không thuộc lớp học này.");

        // Kiểm tra đã có bài nộp chưa (upsert)
        var existing = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == studentId);

        // SQL CHECK constraint: only 'SUBMITTED' or 'GRADED' are valid values.
        // Quá hạn không thay đổi status — đó là logic nghiệp vụ cần giải quyết ở tầng khác.
        const string status = "SUBMITTED";

        if (existing != null)
        {
            // Nộp lại
            existing.FileName     = request.FileName;
            existing.StudentNotes = request.StudentNotes;
            existing.SubmittedAt  = DateTime.Now;
            existing.Status       = status;
            // Reset điểm nếu nộp lại
            existing.Grade    = null;
            existing.Feedback = null;
            existing.GradedAt = null;
        }
        else
        {
            // uniqueidentifier — SQL uses newsequentialid(), EF/DB will generate.
            // We create a new Guid here so the returned DTO has the Id immediately.
            existing = new Submission
            {
                Id           = Guid.NewGuid(),
                AssignmentId = request.AssignmentId,
                StudentId    = studentId,
                FileName     = request.FileName,
                StudentNotes = request.StudentNotes,
                SubmittedAt  = DateTime.Now,
                Status       = status
            };
            _context.Submissions.Add(existing);
        }

        await _context.SaveChangesAsync();
        return MapToDto(existing);
    }

    public async Task<SubmissionGradeDetailDto> GetSubmissionGradeDetailAsync(string submissionId, string studentId)
    {
        // submissionId is a Guid string — parse it
        if (!Guid.TryParse(submissionId, out var submissionGuid))
            throw new ArgumentException("submissionId không hợp lệ.", nameof(submissionId));

        var sub = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionGuid && s.StudentId == studentId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài nộp hoặc bạn không có quyền xem.");

        return new SubmissionGradeDetailDto
        {
            SubmissionId          = sub.Id,
            AssignmentId          = sub.AssignmentId,
            AssignmentTitle       = sub.Assignment.Title,
            AssignmentDescription = sub.Assignment.Description,
            DueDate               = sub.Assignment.DueDate,
            MaxPoints             = sub.Assignment.MaxPoints,
            FileName              = sub.FileName,
            StudentNotes          = sub.StudentNotes,
            SubmittedAt           = sub.SubmittedAt,
            SubmissionStatus      = sub.Status,
            Grade                 = sub.Grade,
            Feedback              = sub.Feedback,
            GradedAt              = sub.GradedAt
        };
    }

    private static SubmissionDto MapToDto(Submission s) => new()
    {
        Id           = s.Id,
        AssignmentId = s.AssignmentId,
        StudentId    = s.StudentId,
        FileName     = s.FileName,
        StudentNotes = s.StudentNotes,
        SubmittedAt  = s.SubmittedAt,
        Status       = s.Status,
        Grade        = s.Grade,
        Feedback     = s.Feedback,
        GradedAt     = s.GradedAt
    };
}
