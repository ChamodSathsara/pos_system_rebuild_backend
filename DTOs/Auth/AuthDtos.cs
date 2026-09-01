namespace PosApi.DTOs.Auth;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public CurrentUserDto User { get; set; } = null!;
}

public class LogoutRequestDto
{
    /// <summary>
    /// Optional. When supplied, only this refresh token is revoked. When omitted, every
    /// active refresh token belonging to the authenticated user is revoked.
    /// </summary>
    public string? RefreshToken { get; set; }
}

public class CurrentUserDto
{
    public string UserCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? BranchCode { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
}
