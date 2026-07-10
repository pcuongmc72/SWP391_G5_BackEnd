using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using SWP.BLL.DTOs.Auth;
using SWP.BLL.Interfaces;
using SWP.BLL.DTOs.Users;
using SWP.DAL.Context;
using SWP.DAL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SWP.BLL.Services;

public class AuthService : IAuthService
{
    private readonly FlippedClassroomContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;

    public AuthService(FlippedClassroomContext context, IConfiguration configuration, IMemoryCache cache, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _cache = cache;
        _emailService = emailService;
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

    // ── ForgotPassword ────────────────────────────────────────────────────────
    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, string ipAddress, string clientOrigin)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // 1. Rate Limiting Check (IP & Email) - 2 phút
        string emailCacheKey = $"rate-limit:forgot-password:email:{email}";
        string ipCacheKey = $"rate-limit:forgot-password:ip:{ipAddress}";

        if (_cache.TryGetValue(emailCacheKey, out _) || _cache.TryGetValue(ipCacheKey, out _))
        {
            throw new InvalidOperationException("Vui lòng đợi 2 phút trước khi yêu cầu lại.");
        }

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        };
        _cache.Set(emailCacheKey, true, cacheOptions);
        _cache.Set(ipCacheKey, true, cacheOptions);

        // 2. Anti-Account Enumeration: Kiểm tra User
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !user.IsActive)
        {
            Console.WriteLine($"[Forgot Password Mock] Email không tồn tại hoặc bị khóa: {email}");
            return;
        }

        // 3. Cryptographically Secure Token Generation
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
        string rawToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        // SHA256 Hash Token
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        string tokenHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        // 4. Lưu DB
        user.PasswordResetTokenHash = tokenHash;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(10);
        await _context.SaveChangesAsync();

        // 5. Gửi email thật (MailKit)
        string resetLink = $"{clientOrigin}/reset-password?token={rawToken}";
        string htmlBody = $"""
            <div style="font-family: Arial, sans-serif; max-width: 520px; margin: auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;">
              <h2 style="color: #0D3E26;">🔒 Khôi phục mật khẩu</h2>
              <p>Xin chào,</p>
              <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản EduTraining của bạn.</p>
              <p>Vui lòng click vào nút bên dưới để đặt lại mật khẩu. Liên kết chỉ có hiệu lực trong <strong>10 phút</strong> và chỉ dùng được <strong>1 lần</strong>.</p>
              <div style="text-align: center; margin: 28px 0;">
                <a href="{resetLink}" style="background: #0D3E26; color: #fff; padding: 12px 28px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 15px;">Đặt lại mật khẩu</a>
              </div>
              <p style="color: #64748b; font-size: 13px;">Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email này. Tài khoản của bạn vẫn an toàn.</p>
              <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;"/>
              <p style="color: #94a3b8; font-size: 12px;">EduTraining — Hệ thống quản lý học tập</p>
            </div>
            """;

        await _emailService.SendEmailAsync(user.Email, "[EduTraining] Yêu cầu khôi phục mật khẩu", htmlBody);

        // Debug log (xóa khi deploy production)
        Console.WriteLine($"[Email] Reset link sent to: {email} → {resetLink}");
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────
    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var rawToken = request.Token.Trim();

        // 1. Hash incoming token
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        string tokenHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        // 2. Tìm match trong DB
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash);
        if (user == null)
        {
            throw new ArgumentException("Token không hợp lệ hoặc đã qua sử dụng.");
        }

        // 3. Kiểm tra hạn token
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            throw new ArgumentException("Token đã hết hạn.");
        }

        // 4. Mã hóa & cập nhật mật khẩu mới
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // 5. Vô hiệu hóa token ngay lập tức (Single-Use)
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        Console.WriteLine($"[Reset Password] Reset thành công cho user: {user.Email}");
    }

    // ── ChangePassword ────────────────────────────────────────────────────────
    public async Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Không tìm thấy người dùng.");
        }

        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException("Mật khẩu hiện tại không chính xác.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        Console.WriteLine($"[Change Password] Đổi mật khẩu thành công cho user: {user.Email}");
    }
}
