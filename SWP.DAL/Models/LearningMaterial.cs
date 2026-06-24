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

    /// <summary>Soft delete flag – true means disabled/hidden from students but still visible to lecturer</summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>Tên chương / chapter của học liệu. Format: "TênMôn ÷ TênChương" (ví dụ: "SWP ÷ Chương 1")</summary>
    public string? Chapter { get; set; }

    /// <summary>Tên bài học cụ thể (tuỳ chọn, có thể null)</summary>
    public string? Lesson { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<MaterialCompletion> MaterialCompletions { get; set; } = new List<MaterialCompletion>();
}
