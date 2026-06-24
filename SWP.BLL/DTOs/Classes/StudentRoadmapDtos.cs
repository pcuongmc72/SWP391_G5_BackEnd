using System;
using System.Collections.Generic;

namespace SWP.BLL.DTOs.Classes;

public class StudentRoadmapMaterialDto
{
    public Guid Id { get; set; }
    public string ClassId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public string? FileUrl { get; set; }
    public string? FileSize { get; set; }
    public DateOnly UploadedAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ChapterRoadmapDto
{
    public string ChapterName { get; set; } = null!;
    public List<StudentRoadmapMaterialDto> Materials { get; set; } = new();
}

public class StudentClassRoadmapDto
{
    public string ClassId { get; set; } = null!;
    public string ClassName { get; set; } = null!;
    public List<ChapterRoadmapDto> Chapters { get; set; } = new();
}
