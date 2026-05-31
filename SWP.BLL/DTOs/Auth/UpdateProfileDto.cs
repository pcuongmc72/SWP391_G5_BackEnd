using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Auth;

public class UpdateProfileDto
{
    [MaxLength(255)]
    public string? FullName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }
}
