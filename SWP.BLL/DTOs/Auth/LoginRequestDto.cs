using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email la bat buoc")]
    [EmailAddress(ErrorMessage = "Email khong hop le")]
<<<<<<< HEAD
    [MaxLength(100)]
=======
    [MaxLength(255)]
>>>>>>> origin/thuanpdhe187333
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password la bat buoc")]
    public string Password { get; set; } = null!;
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/thuanpdhe187333
