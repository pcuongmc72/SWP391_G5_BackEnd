using System;
using System.Collections.Generic;

namespace SWP.DAL.Models;

public partial class User
{
<<<<<<< HEAD
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Lecturer? Lecturer { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Student? Student { get; set; }
=======
    public string Id { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
>>>>>>> origin/thuanpdhe187333
}
