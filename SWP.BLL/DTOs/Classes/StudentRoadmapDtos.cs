using System;
using System.Collections.Generic;

namespace SWP.BLL.DTOs.Classes;

/// <summary>
/// DTO trả về toàn bộ lộ trình học tập của một lớp, nhóm theo Chapter.
/// </summary>
public class StudentClassRoadmapDto
{
    public string ClassId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public List<ChapterRoadmapDto> Chapters { get; set; } = new();
}

/// <summary>
/// DTO cho một Chapter (chương học), chứa danh sách học liệu trong chương đó.
/// </summary>
public class ChapterRoadmapDto
{
    public string ChapterName { get; set; } = string.Empty;
    public List<StudentRoadmapMaterialDto> Materials { get; set; } = new();
}

/// <summary>
/// DTO cho một học liệu trong lộ trình, bao gồm trạng thái hoàn thành của sinh viên.
/// </summary>
public class StudentRoadmapMaterialDto
{
    public Guid Id { get; set; }
    public string ClassId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileSize { get; set; }
    public DateOnly UploadedAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
