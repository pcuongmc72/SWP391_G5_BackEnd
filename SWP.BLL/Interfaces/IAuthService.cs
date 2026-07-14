using SWP.BLL.DTOs.Auth;
using SWP.BLL.DTOs.Users;

namespace SWP.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserInfoDto> GetProfileAsync(string id);
    Task<UserInfoDto> UpdateProfileAsync(string id, UpdateProfileDto request);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request, string ipAddress, string clientOrigin);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
    Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
}
