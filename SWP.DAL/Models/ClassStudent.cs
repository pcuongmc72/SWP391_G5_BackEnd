namespace SWP.DAL.Models;

public partial class ClassStudent
{
    public string ClassId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime EnrolledAt { get; set; }

    /// <summary>ClassRole: student | assistant</summary>
    public string ClassRole { get; set; } = "student";

    public virtual Class Class { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
