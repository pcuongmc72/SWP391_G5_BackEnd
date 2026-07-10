using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Users;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Ma dinh danh (Id) la bat buoc.")]
    [RegularExpression(@"^[A-Z]{2}\d{6}$", ErrorMessage = "Id phai bat dau bang 2 chu cai in hoa va theo sau la dung 6 chu so (VD: HE187159, GV123456).")]
    [MaxLength(20)]
    public string Id { get; set; } = null!;

    [Required(ErrorMessage = "Email la bat buoc.")]
    [EmailAddress(ErrorMessage = "Email khong hop le.")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password la bat buoc.")]
    [MinLength(6, ErrorMessage = "Password phai co it nhat 6 ky tu.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Ho va ten la bat buoc.")]
    [MaxLength(255)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Role la bat buoc.")]
    [RegularExpression("^(Admin|Lecturer|Student)$", ErrorMessage = "Role chi duoc phep la: Admin, Lecturer, Student.")]
    [MaxLength(20)]
    public string Role { get; set; } = null!;

    public string? AvatarUrl { get; set; }
}