namespace SWP.BLL.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public UserInfoDto User { get; set; } = null!;
}

public class UserInfoDto
{
    public string Id { get; set; } = null!;           
    public string FullName { get; set; } = null!;    
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;        
    public bool IsActive { get; set; }
}