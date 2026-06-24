using System;

namespace SWP.BLL.DTOs.Materials;

public class MaterialDto
{
    // uniqueidentifier in SQL
    public Guid Id { get; set; }

    public string ClassId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    // SQL column: MaterialType  varchar(20) NOT NULL
    public string MaterialType { get; set; } = null!;

    // SQL column: FileUrl  nvarchar(500) NULL
    public string? FileUrl { get; set; }

    public string? FileSize { get; set; }

    // SQL column: UploadedAt  date NOT NULL
    public DateOnly UploadedAt { get; set; }

    // SQL column: CreatedAt  datetime2(0) NOT NULL
    public DateTime CreatedAt { get; set; }

    // Computed: not a SQL column — derived from MaterialCompletions join
    public bool IsCompleted { get; set; }
}
