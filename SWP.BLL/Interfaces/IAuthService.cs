using SWP.BLL.DTOs.Auth;

namespace SWP.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<UserInfoDto> GetProfileAsync(int userId);
}
