using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.LearningMaterials;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class StudentLearningMaterialsService : IStudentLearningMaterialsService
{
    private readonly FlippedClassroomContext _context;

    public StudentLearningMaterialsService(FlippedClassroomContext context)
    {
        _context = context;
    }

    /// <summary>Lấy học liệu của lớp và đánh dấu những cái sinh viên đã hoàn thành.</summary>
    public async Task<IEnumerable<StudentLearningMaterialDto>> GetMaterialsForStudentAsync(string classId, string studentId)
    {
        var materials = await _context.LearningMaterials
            .Where(m => m.ClassId == classId)
            .OrderBy(m => m.UploadedAt)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync();

        var completedIds = new HashSet<Guid>(
            await _context.MaterialCompletions
                .Where(mc => mc.StudentId == studentId && mc.Material.ClassId == classId)
                .Select(mc => mc.MaterialId)
                .ToListAsync()
        );

        return materials.Select(m => new StudentLearningMaterialDto
        {
            Id = m.Id,
            ClassId = m.ClassId,
            Title = m.Title,
            Description = m.Description,
            MaterialType = m.MaterialType,
            FileUrl = m.FileUrl,
            FileSize = m.FileSize,
            UploadedAt = m.UploadedAt.ToString("yyyy-MM-dd"),
            CreatedAt = m.CreatedAt,
            IsCompleted = completedIds.Contains(m.Id)
        });
    }

    /// <summary>Lưu bản ghi hoàn thành vào MaterialCompletions nếu chưa tồn tại.</summary>
    public async Task MarkAsCompletedAsync(Guid materialId, string studentId)
    {
        var exists = await _context.MaterialCompletions
            .AnyAsync(mc => mc.MaterialId == materialId && mc.StudentId == studentId);

        if (exists) return;

        _context.MaterialCompletions.Add(new MaterialCompletion
        {
            MaterialId = materialId,
            StudentId = studentId,
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    /// <summary>Xóa bản ghi hoàn thành khỏi MaterialCompletions.</summary>
    public async Task UnmarkAsCompletedAsync(Guid materialId, string studentId)
    {
        var completion = await _context.MaterialCompletions
            .FirstOrDefaultAsync(mc => mc.MaterialId == materialId && mc.StudentId == studentId);

        if (completion is null) return;

        _context.MaterialCompletions.Remove(completion);
        await _context.SaveChangesAsync();
    }
}
