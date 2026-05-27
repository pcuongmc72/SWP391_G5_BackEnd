using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Auth;

public class RegisterRequestDto
{
<<<<<<< HEAD
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
=======
    [Required(ErrorMessage = "Ma dinh danh (Id) la bat buoc.")]
    // Bắt buộc ID phải bắt đầu bằng 2 chữ cái, theo sau là các chữ số
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
    [RegularExpression("^(admin|lecturer|student)$", ErrorMessage = "Role chi duoc phep la: admin, lecturer, student.")]
    [MaxLength(20)]
    public string Role { get; set; } = null!;
}
>>>>>>> origin/thuanpdhe187333
