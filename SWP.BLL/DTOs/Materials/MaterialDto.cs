using System;

namespace SWP.BLL.DTOs.Materials;

public class MaterialDto
{
    public string Id { get; set; } = null!;

    public string ClassId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Type { get; set; } = null!; // 'video' | 'pdf' | 'document' | 'quiz'

    public string Url { get; set; } = null!;

    public string? FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    public bool IsCompleted { get; set; }
}
