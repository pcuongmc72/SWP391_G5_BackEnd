<<<<<<< HEAD
=======
using System;

>>>>>>> origin/cuongnphe194338
namespace SWP.DAL.Models;

public partial class MaterialCompletion
{
    public Guid MaterialId { get; set; }
<<<<<<< HEAD

    public string StudentId { get; set; } = null!;

    public DateTime CompletedAt { get; set; }

    public virtual LearningMaterial Material { get; set; } = null!;

=======
    public string StudentId { get; set; } = null!;
    public DateTime CompletedAt { get; set; }

    public virtual LearningMaterial Material { get; set; } = null!;
>>>>>>> origin/cuongnphe194338
    public virtual User Student { get; set; } = null!;
}
