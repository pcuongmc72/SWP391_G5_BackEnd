using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Username là bắt buộc")]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password là bắt buộc")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// RoleId: 1 = Admin, 2 = Lecturer, 3 = Student
    /// </summary>
    [Required(ErrorMessage = "RoleId là bắt buộc")]
    public int RoleId { get; set; }
}
