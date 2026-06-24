using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Materials;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class MaterialsService : IMaterialsService
{
    private readonly FlippedClassroomContext _context;

    public MaterialsService(FlippedClassroomContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MaterialDto>> GetMaterialsByClassAsync(string classId, string studentId)
    {
        // Lấy tất cả tài liệu của lớp học này
        var materials = await _context.Materials
            .Where(m => m.ClassId == classId)
            .OrderBy(m => m.UploadedAt)
            .ToListAsync();

        // Lấy các MaterialId (Guid) mà học viên đã hoàn thành trong lớp học này
        var completedMaterialIds = await _context.MaterialCompletions
            .Where(mc => mc.StudentId == studentId && mc.Material.ClassId == classId)
            .Select(mc => mc.MaterialId)   // Guid
            .ToListAsync();

        return materials.Select(m => new MaterialDto
        {
            Id           = m.Id,
            ClassId      = m.ClassId,
            Title        = m.Title,
            Description  = m.Description,
            MaterialType = m.MaterialType,  // SQL column: MaterialType
            FileUrl      = m.FileUrl,       // SQL column: FileUrl
            FileSize     = m.FileSize,
            UploadedAt   = m.UploadedAt,    // DateOnly (SQL: date)
            CreatedAt    = m.CreatedAt,
            IsCompleted  = completedMaterialIds.Contains(m.Id)  // Guid == Guid
        });
    }

    public async Task<bool> MarkMaterialCompleteAsync(string materialId, string studentId)
    {
        if (!Guid.TryParse(materialId, out var materialGuid))
            return false;

        // Kiểm tra xem học liệu này có tồn tại không
        var materialExists = await _context.Materials.AnyAsync(m => m.Id == materialGuid);
        if (!materialExists)
        {
            return false;
        }

        // Kiểm tra xem đã được đánh dấu hoàn thành trước đó chưa
        var alreadyCompleted = await _context.MaterialCompletions
            .AnyAsync(mc => mc.MaterialId == materialGuid && mc.StudentId == studentId);

        if (!alreadyCompleted)
        {
            var completion = new MaterialCompletion
            {
                MaterialId  = materialGuid,   // Guid
                StudentId   = studentId,
                CompletedAt = DateTime.UtcNow
            };

            _context.MaterialCompletions.Add(completion);
            await _context.SaveChangesAsync();
        }

        return true;
    }
}
