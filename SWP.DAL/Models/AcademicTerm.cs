using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class AcademicTerm
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>Mã học kỳ, VD: "HK1-2024-2025"</summary>
    public string TermCode { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
