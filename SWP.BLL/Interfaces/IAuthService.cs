using SWP.BLL.DTOs.Auth;

namespace SWP.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserInfoDto> GetProfileAsync(string id);
}
