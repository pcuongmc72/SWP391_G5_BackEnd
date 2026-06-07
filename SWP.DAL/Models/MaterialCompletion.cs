using System;

namespace SWP.DAL.Models;

public partial class MaterialCompletion
{
    public string MaterialId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime CompletedAt { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
