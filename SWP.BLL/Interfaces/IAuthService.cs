using SWP.BLL.DTOs.Auth;

namespace SWP.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
<<<<<<< HEAD
    Task<UserInfoDto> GetProfileAsync(int userId);
}
=======
    Task<UserInfoDto> GetProfileAsync(String id);
}
>>>>>>> origin/thuanpdhe187333
