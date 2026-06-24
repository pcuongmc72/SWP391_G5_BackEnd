using System;

namespace SWP.DAL.Models;

// SQL table: [dbo].[MaterialCompletions]
public partial class MaterialCompletion
{
    // uniqueidentifier NOT NULL  FK → LearningMaterials.Id
    public Guid MaterialId { get; set; }

    // varchar(20) NOT NULL  FK → Users.Id
    public string StudentId { get; set; } = null!;

    // datetime2(0) NOT NULL DEFAULT (sysdatetime())
    public DateTime CompletedAt { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
