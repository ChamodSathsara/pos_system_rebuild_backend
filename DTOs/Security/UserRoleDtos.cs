namespace PosApi.DTOs.Security;

public class CreateUserRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateUserRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UserRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Role detail including the permissions currently assigned to it.
/// </summary>
public class UserRoleWithPermissionsDto : UserRoleDto
{
    public List<PermissionDto> Permissions { get; set; } = new();
}
