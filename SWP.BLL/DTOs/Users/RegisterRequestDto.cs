using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Users;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Ma dinh danh (Id) la bat buoc.")]
    [RegularExpression(@"^[a-zA-Z]{2}\d+$", ErrorMessage = "Id phai bat dau bang 2 chu cai va theo sau la so (VD: HE187159, GV123456).")]
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
    // Ép luôn Role chỉ được nhập đúng 3 chữ này
    [RegularExpression("^(Admin|Lecturer|Student)$", ErrorMessage = "Role chi duoc phep la: Admin, Lecturer, Student.")]
    [MaxLength(20)]
    public string Role { get; set; } = null!;
}