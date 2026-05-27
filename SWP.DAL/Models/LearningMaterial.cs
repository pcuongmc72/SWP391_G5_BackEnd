namespace SWP.DAL.Models;

public partial class LearningMaterial
{
    public Guid Id { get; set; }

    public string ClassId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string MaterialType { get; set; } = null!;

    public string? FileUrl { get; set; }

    public string? FileSize { get; set; }

    public DateOnly UploadedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<MaterialCompletion> MaterialCompletions { get; set; } = new List<MaterialCompletion>();
}
