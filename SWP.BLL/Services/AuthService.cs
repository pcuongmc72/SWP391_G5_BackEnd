<<<<<<< HEAD
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
=======
﻿using Microsoft.EntityFrameworkCore;
>>>>>>> origin/thuanpdhe187333
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SWP.BLL.DTOs.Auth;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;
<<<<<<< HEAD
=======
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
>>>>>>> origin/thuanpdhe187333

namespace SWP.BLL.Services;

public class AuthService : IAuthService
{
<<<<<<< HEAD
    private readonly EduTrainingDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(EduTrainingDbContext context, IConfiguration configuration)
=======
    private readonly FlippedClassroomContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(FlippedClassroomContext context, IConfiguration configuration)
>>>>>>> origin/thuanpdhe187333
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
<<<<<<< HEAD
            .Include(u => u.Role)
=======
>>>>>>> origin/thuanpdhe187333
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

<<<<<<< HEAD
=======
        // So sánh chuỗi thường dành cho tài khoản test
>>>>>>> origin/thuanpdhe187333
        return string.Equals(inputPassword, storedHash, StringComparison.Ordinal);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
<<<<<<< HEAD
        bool usernameExists = await _context.Users.AnyAsync(u => u.Username == request.Username);
        if (usernameExists)
            throw new InvalidOperationException("Username da ton tai.");

=======
        // 1. Kiểm tra ID có bị trùng không (Vì ID giờ là mã SV/GV tự nhập)
        bool idExists = await _context.Users.AnyAsync(u => u.Id == request.Id);
        if (idExists)
            throw new InvalidOperationException("Ma dinh danh (Id) da ton tai.");

        // 2. Kiểm tra Email
>>>>>>> origin/thuanpdhe187333
        bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            throw new InvalidOperationException("Email da duoc su dung.");

<<<<<<< HEAD
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
=======
        // 3. Kiểm tra Role hợp lệ theo Constraint CHECK trong DB
        var validRoles = new[] { "admin", "lecturer", "student" };
        if (!validRoles.Contains(request.Role.ToLower()))
            throw new InvalidOperationException("Role khong hop le (chi nhan: admin, lecturer, student).");

        var newUser = new User
        {
            Id = request.Id, // VD: HE187159
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role.ToLower(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
>>>>>>> origin/thuanpdhe187333
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

<<<<<<< HEAD
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
=======
        return BuildAuthResponse(newUser);
    }

    // Đổi tham số từ int sang string vì Id giờ là VARCHAR(20)
    public async Task<UserInfoDto> GetProfileAsync(string id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
>>>>>>> origin/thuanpdhe187333

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
<<<<<<< HEAD
        UserId = user.UserId,
        Username = user.Username,
        Email = user.Email,
        RoleName = user.Role?.RoleName ?? string.Empty,
        IsActive = user.IsActive ?? false
=======
        Id = user.Id,                 // Thay vì UserId
        Email = user.Email,
        FullName = user.FullName,     // Thay vì Username
        Role = user.Role,             // Thay vì RoleName
        IsActive = user.IsActive
>>>>>>> origin/thuanpdhe187333
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
<<<<<<< HEAD
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? string.Empty),
=======
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),               // Map mã SV/GV vào Sub
            new Claim(JwtRegisteredClaimNames.Email, user.Email),          // Lưu Email
            new Claim(ClaimTypes.Name, user.FullName),                     // Lưu FullName
            new Claim(ClaimTypes.Role, user.Role),                         // Lưu quyền (Role) trực tiếp
>>>>>>> origin/thuanpdhe187333
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
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/thuanpdhe187333
