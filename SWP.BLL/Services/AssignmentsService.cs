using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Assignments;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;

namespace SWP.BLL.Services;

public class AssignmentsService : IAssignmentsService
{
    private readonly FlippedClassroomContext _context;

    public AssignmentsService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssignmentDto>> GetAssignmentsByClassAsync(string classId, string studentId)
    {
        var assignments = await _context.Assignments
            .Where(a => a.ClassId == classId)
            .OrderBy(a => a.DueDate)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();

        // Lấy tất cả submission của sinh viên này cho lớp này
        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var mySubmissions = await _context.Submissions
            .Where(s => s.StudentId == studentId && assignmentIds.Contains(s.AssignmentId))
            .ToDictionaryAsync(s => s.AssignmentId);

        return assignments.Select(a =>
        {
            mySubmissions.TryGetValue(a.Id, out var sub);
            return new AssignmentDto
            {
                Id          = a.Id,
                ClassId     = a.ClassId,
                Title       = a.Title,
                Description = a.Description,
                DueDate     = a.DueDate,
                MaxPoints   = a.MaxPoints,
                CreatedAt   = a.CreatedAt,
                MySubmission = sub == null ? null : new SubmissionStatusDto
                {
                    Id           = sub.Id,
                    FileName     = sub.FileName,
                    // FileUrl removed — not a SQL column in Submissions
                    StudentNotes = sub.StudentNotes,
                    SubmittedAt  = sub.SubmittedAt,
                    Status       = sub.Status,
                    Grade        = sub.Grade,
                    Feedback     = sub.Feedback,
                    GradedAt     = sub.GradedAt
                }
            };
        });
    }

    public async Task<AssignmentDto> GetAssignmentByIdAsync(string assignmentId, string studentId)
    {
        if (!Guid.TryParse(assignmentId, out var assignmentGuid))
            throw new ArgumentException("assignmentId không hợp lệ.", nameof(assignmentId));

        var a = await _context.Assignments
            .FirstOrDefaultAsync(x => x.Id == assignmentGuid)
            ?? throw new KeyNotFoundException("Không tìm thấy bài tập.");

        var sub = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentGuid && s.StudentId == studentId);

        return new AssignmentDto
        {
            Id          = a.Id,
            ClassId     = a.ClassId,
            Title       = a.Title,
            Description = a.Description,
            DueDate     = a.DueDate,
            MaxPoints   = a.MaxPoints,
            CreatedAt   = a.CreatedAt,
            MySubmission = sub == null ? null : new SubmissionStatusDto
            {
                Id           = sub.Id,
                FileName     = sub.FileName,
                // FileUrl removed — not a SQL column in Submissions
                StudentNotes = sub.StudentNotes,
                SubmittedAt  = sub.SubmittedAt,
                Status       = sub.Status,
                Grade        = sub.Grade,
                Feedback     = sub.Feedback,
                GradedAt     = sub.GradedAt
            }
        };
    }
}
