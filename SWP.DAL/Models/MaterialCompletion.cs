using System;

namespace SWP.DAL.Models;

public partial class MaterialCompletion
{
    public Guid MaterialId { get; set; }
    public string StudentId { get; set; } = null!;
    public DateTime CompletedAt { get; set; }

    public virtual LearningMaterial Material { get; set; } = null!;
    public virtual User Student { get; set; } = null!;
}
