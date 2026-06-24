using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

// SQL table: [dbo].[Assignments]
public partial class Assignment
{
    // uniqueidentifier NOT NULL DEFAULT (newsequentialid())
    public Guid Id { get; set; }

    // varchar(20) NOT NULL  FK → Classes
    public string ClassId { get; set; } = null!;

    // nvarchar(255) NOT NULL
    public string Title { get; set; } = null!;

    // nvarchar(max) NULL
    public string? Description { get; set; }

    // date NOT NULL
    public DateOnly DueDate { get; set; }

    // decimal(5,2) NOT NULL DEFAULT ((10))
    public decimal MaxPoints { get; set; } = 10;

    // datetime2(7) NOT NULL DEFAULT (sysdatetime())
    public DateTime CreatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
