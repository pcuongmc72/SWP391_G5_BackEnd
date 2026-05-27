using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Auth;
using SWP.BLL.Interfaces;
<<<<<<< HEAD
=======
using System.IdentityModel.Tokens.Jwt;
>>>>>>> origin/thuanpdhe187333
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
    //  POST /api/auth/register
    // ──────────────────────────────────────────────
<<<<<<< HEAD
    /// <summary>Đăng ký tài khoản mới</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), 201)]
=======
    /// <summary>Tạo tài khoản mới (Chỉ Admin mới có quyền)</summary>
    [HttpPost("register")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
>>>>>>> origin/thuanpdhe187333
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
<<<<<<< HEAD
            return CreatedAtAction(nameof(GetProfile),
                new { },
                new { success = true, message = "Đăng ký thành công.", data = result });
=======
            return Ok(new { success = true, message = "Tạo tài khoản thành công.", data = result });
>>>>>>> origin/thuanpdhe187333
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
<<<<<<< HEAD
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (userIdClaim is null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var profile = await _authService.GetProfileAsync(userId);
=======
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile()
    {
        // Đã sửa: Chỉ lấy chuỗi string ra, không parse sang int nữa
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value
                       ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc không tìm thấy ID." });

        try
        {
            // Truyền thẳng chuỗi ID (VD: "HE187159") xuống Service
            var profile = await _authService.GetProfileAsync(userIdClaim);
>>>>>>> origin/thuanpdhe187333
            return Ok(new { success = true, data = profile });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/thuanpdhe187333
