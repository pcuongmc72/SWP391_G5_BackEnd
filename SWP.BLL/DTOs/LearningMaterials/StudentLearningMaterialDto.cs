using System;

namespace SWP.BLL.DTOs.LearningMaterials;

public class StudentLearningMaterialDto
{
    public Guid Id { get; set; }
    public string ClassId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string MaterialType { get; set; } = null!;
    public string? FileUrl { get; set; }
    public string? FileSize { get; set; }
    public string UploadedAt { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }
}
