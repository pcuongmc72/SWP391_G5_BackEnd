using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Material
{
    public string Id { get; set; } = null!;

    public string ClassId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Type { get; set; } = null!; // 'video' | 'pdf' | 'document' | 'quiz'

    public string Url { get; set; } = null!;

    public string? FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<MaterialCompletion> MaterialCompletions { get; set; } = new List<MaterialCompletion>();
}
