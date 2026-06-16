namespace SWP.DAL.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    /// <summary>Role: admin | lecturer | student</summary>
    public string Role { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Bio { get; set; }
    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
