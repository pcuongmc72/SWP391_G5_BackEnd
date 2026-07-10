using System.ComponentModel.DataAnnotations;

namespace SWP.BLL.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Token là bắt buộc.")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải từ 6 ký tự trở lên.")]
    public string NewPassword { get; set; } = null!;
}
