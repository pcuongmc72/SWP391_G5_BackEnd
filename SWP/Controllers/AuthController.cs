using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Auth;
using SWP.BLL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ──────────────────────────────────────────────
    //  POST /api/auth/login
    // ──────────────────────────────────────────────
    /// <summary>Đăng nhập và nhận JWT token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(new { success = true, message = "Đăng nhập thành công.", data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { success = false, message = ex.Message });
        }
    }

    // ──────────────────────────────────────────────
    //  POST /api/auth/register  (Admin only)
    // ──────────────────────────────────────────────
    /// <summary>Tạo tài khoản mới — chỉ Admin được phép</summary>
    [HttpPost("register")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(new { success = true, message = "Tạo tài khoản thành công.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ──────────────────────────────────────────────
    //  GET /api/auth/profile
    // ──────────────────────────────────────────────
    /// <summary>Lấy thông tin người dùng hiện tại (yêu cầu JWT)</summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile()
    {
        var id = GetCurrentUserId();
        if (id is null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var profile = await _authService.GetProfileAsync(id);
            return Ok(new { success = true, data = profile });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    // ──────────────────────────────────────────────
    //  PUT /api/auth/profile
    // ──────────────────────────────────────────────
    /// <summary>Cập nhật thông tin cá nhân (FullName, Phone, Address, Bio, AvatarUrl)</summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        var id = GetCurrentUserId();
        if (id is null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var updated = await _authService.UpdateProfileAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật thành công.", data = updated });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    // ── Helper ────────────────────────────────────
    private string? GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}
