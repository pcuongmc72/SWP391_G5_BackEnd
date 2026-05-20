using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SWP.BLL.DTOs.Auth;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;

namespace SWP.BLL.Services;

public class AuthService : IAuthService
{
    private readonly EduTrainingDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(EduTrainingDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            throw new UnauthorizedAccessException("Email hoac password khong dung.");

        if (user.IsActive == false)
            throw new UnauthorizedAccessException("Tai khoan da bi vo hieu hoa.");

        bool passwordValid = VerifyPassword(request.Password, user.PasswordHash);
        if (!passwordValid)
            throw new UnauthorizedAccessException("Email hoac password khong dung.");

        if (!IsBCryptHash(user.PasswordHash))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            await _context.SaveChangesAsync();
        }

        return BuildAuthResponse(user);
    }

    private static bool IsBCryptHash(string hash) =>
        hash.StartsWith("$2a$") || hash.StartsWith("$2b$") ||
        hash.StartsWith("$2x$") || hash.StartsWith("$2y$");

    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        if (IsBCryptHash(storedHash))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        return string.Equals(inputPassword, storedHash, StringComparison.Ordinal);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        bool usernameExists = await _context.Users.AnyAsync(u => u.Username == request.Username);
        if (usernameExists)
            throw new InvalidOperationException("Username da ton tai.");

        bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            throw new InvalidOperationException("Email da duoc su dung.");

        bool roleExists = await _context.Roles.AnyAsync(r => r.RoleId == request.RoleId);
        if (!roleExists)
            throw new InvalidOperationException($"RoleId {request.RoleId} khong ton tai.");

        var newUser = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Email = request.Email,
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var createdUser = await _context.Users
            .Include(u => u.Role)
            .FirstAsync(u => u.UserId == newUser.UserId);

        return BuildAuthResponse(createdUser);
    }

    public async Task<UserInfoDto> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
            throw new KeyNotFoundException("Nguoi dung khong ton tai.");

        return MapToUserInfo(user);
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var (token, expiresAt) = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            User = MapToUserInfo(user)
        };
    }

    private static UserInfoDto MapToUserInfo(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        RoleName = user.Role?.RoleName ?? string.Empty,
        IsActive = user.IsActive ?? false
    };

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        int expireMinutes = int.Parse(jwtSection["ExpireMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
