using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SWP.BLL.DTOs.Auth;
using SWP.BLL.Interfaces;
using SWP.BLL.DTOs.Users;
using SWP.DAL.Context;
using SWP.DAL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SWP.BLL.Services;

public class AuthService : IAuthService
{
    private readonly FlippedClassroomContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(FlippedClassroomContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            throw new UnauthorizedAccessException("Email hoac password khong dung.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tai khoan da bi vo hieu hoa.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoac password khong dung.");

        // Tự động nâng cấp hash mật khẩu thường → BCrypt
        if (!IsBCryptHash(user.PasswordHash))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            await _context.SaveChangesAsync();
        }

        return BuildAuthResponse(user);
    }

    // ── GetProfile ────────────────────────────────────────────────────────────
    public async Task<UserInfoDto> GetProfileAsync(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new KeyNotFoundException("Nguoi dung khong ton tai.");

        return MapToUserInfo(user);
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────
    public async Task<UserInfoDto> UpdateProfileAsync(string id, UpdateProfileDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new KeyNotFoundException("Nguoi dung khong ton tai.");

        // Chỉ cập nhật các field cho phép sửa
        user.FullName  = request.FullName  ?? user.FullName;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
        user.Phone     = request.Phone     ?? user.Phone;
        user.Address   = request.Address   ?? user.Address;
        user.Bio       = request.Bio       ?? user.Bio;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToUserInfo(user);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private AuthResponseDto BuildAuthResponse(User user)
    {
        var (token, expiresAt) = GenerateJwtToken(user);
        return new AuthResponseDto
        {
            Token     = token,
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            User      = MapToUserInfo(user)
        };
    }

    private static UserInfoDto MapToUserInfo(User user) => new()
    {
        Id        = user.Id,
        FullName  = user.FullName,
        Email     = user.Email,
        Role      = user.Role,
        AvatarUrl = user.AvatarUrl,
        IsActive  = user.IsActive,
        Phone     = user.Phone,
        Address   = user.Address,
        Bio       = user.Bio
    };

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
    {
        var jwtSection    = _configuration.GetSection("Jwt");
        var key           = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        int expireMinutes = int.Parse(jwtSection["ExpireMinutes"] ?? "60");
        var expiresAt     = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name,               user.FullName),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             jwtSection["Issuer"],
            audience:           jwtSection["Audience"],
            claims:             claims,
            expires:            expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private static bool IsBCryptHash(string hash) =>
        hash.StartsWith("$2a$") || hash.StartsWith("$2b$") ||
        hash.StartsWith("$2x$") || hash.StartsWith("$2y$");

    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        if (IsBCryptHash(storedHash))
        {
            try   { return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash); }
            catch (BCrypt.Net.SaltParseException) { return false; }
        }
        // Plain-text so sánh thẳng (tài khoản test chưa hash)
        return string.Equals(inputPassword, storedHash, StringComparison.Ordinal);
    }
}
