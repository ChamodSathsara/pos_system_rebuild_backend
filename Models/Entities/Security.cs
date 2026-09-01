namespace PosApi.Models.Entities;

/// <summary>
/// Replaces the original "user_group" table. Represents a role assignable to a system user
/// (e.g. Admin, Cashier, Manager). Used for role-based authorization and JWT "role" claims.
/// </summary>
public class UserRole
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }

    public ICollection<UserRolePermission> UserRolePermissions { get; set; } = new List<UserRolePermission>();
    public ICollection<SystemUser> SystemUsers { get; set; } = new List<SystemUser>();
}

public class Permission
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<UserRolePermission> UserRolePermissions { get; set; } = new List<UserRolePermission>();
}

/// <summary>
/// Replaces the original "user_group_permission" join table.
/// </summary>
public class UserRolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    public UserRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public class SystemUser
{
    public string UserCode { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? BranchCode { get; set; }
    public int? RoleId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Branch? Branch { get; set; }
    public UserRole? Role { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}

public class RefreshToken
{
    public int Id { get; set; }
    public string UserCode { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public SystemUser? User { get; set; }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
}

public class PasswordResetToken
{
    public int Id { get; set; }
    public string UserCode { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public SystemUser? User { get; set; }
}
