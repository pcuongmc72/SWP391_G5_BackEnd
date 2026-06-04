using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class Class
{
    public string Id { get; set; } = null!;

    public Guid CourseId { get; set; }

    public Guid AcademicTermId { get; set; }

    public string? LecturerId { get; set; }

    public bool AllowReviewAfterEnd { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AcademicTerm AcademicTerm { get; set; } = null!;

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual Course Course { get; set; } = null!;

    public virtual User? Lecturer { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
