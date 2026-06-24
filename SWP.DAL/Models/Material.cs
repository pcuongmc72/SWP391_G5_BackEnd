using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

// SQL table: [dbo].[LearningMaterials]
public partial class Material
{
    // uniqueidentifier NOT NULL DEFAULT (newsequentialid())
    public Guid Id { get; set; }

    // varchar(20) NOT NULL  FK → Classes
    public string ClassId { get; set; } = null!;

    // nvarchar(255) NOT NULL
    public string Title { get; set; } = null!;

    // nvarchar(max) NULL
    public string? Description { get; set; }

    // varchar(20) NOT NULL  CHECK: 'quiz'|'document'|'pdf'|'video'
    public string MaterialType { get; set; } = null!;

    // nvarchar(500) NULL
    public string? FileUrl { get; set; }

    // nvarchar(50) NULL
    public string? FileSize { get; set; }

    // date NOT NULL DEFAULT CONVERT([date], sysdatetime())
    public DateOnly UploadedAt { get; set; }

    // datetime2(0) NOT NULL DEFAULT (sysdatetime())
    public DateTime CreatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<MaterialCompletion> MaterialCompletions { get; set; } = new List<MaterialCompletion>();
}
