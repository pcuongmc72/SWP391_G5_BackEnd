using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PasswordHash { get; set; }

    /// <summary>Vai trò: "admin" | "lecturer" | "student"</summary>
    public string? Role { get; set; }

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
}
