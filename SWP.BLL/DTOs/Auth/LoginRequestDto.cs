using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email la bat buoc")]
    [EmailAddress(ErrorMessage = "Email khong hop le")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password la bat buoc")]
    public string? Password { get; set; } = null!;
}