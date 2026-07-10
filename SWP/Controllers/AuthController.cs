using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Auth;
using SWP.BLL.DTOs.Users;
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

    //  POST /api/auth/login
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
    //  GET /api/auth/profile
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

    // ──────────────────────────────────────────────
    //  POST /api/auth/forgot-password
    // ──────────────────────────────────────────────
    [HttpPost("forgot-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        var origin = Request.Headers["Origin"].ToString();
        if (string.IsNullOrEmpty(origin))
        {
            origin = Request.Headers["Referer"].ToString();
        }
        if (string.IsNullOrEmpty(origin))
        {
            origin = "http://localhost:5173"; // Vite default fallback
        }
        origin = origin.TrimEnd('/');

        try
        {
            await _authService.ForgotPasswordAsync(request, ipAddress, origin);
            return Ok(new { success = true, message = "Nếu email tồn tại trong hệ thống, link khôi phục mật khẩu đã được gửi đi." });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(429, new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ──────────────────────────────────────────────
    //  POST /api/auth/reset-password
    // ──────────────────────────────────────────────
    [HttpPost("reset-password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new { success = true, message = "Mật khẩu đã được đặt lại thành công." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "Có lỗi xảy ra trong quá trình đặt lại mật khẩu." });
        }
    }

    // ──────────────────────────────────────────────
    //  POST /api/auth/change-password
    // ──────────────────────────────────────────────
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var id = GetCurrentUserId();
        if (id is null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc đã hết hạn." });

        try
        {
            await _authService.ChangePasswordAsync(id, request);
            return Ok(new { success = true, message = "Mật khẩu đã được thay đổi thành công." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "Có lỗi xảy ra trong quá trình đổi mật khẩu." });
        }
    }

    // ── Helper ────────────────────────────────────
    private string? GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}
