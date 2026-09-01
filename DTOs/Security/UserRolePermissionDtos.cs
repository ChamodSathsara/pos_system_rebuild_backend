namespace PosApi.DTOs.Security;

/// <summary>
/// Assigns an existing permission to an existing role. RoleId is taken from the route.
/// </summary>
public class AssignPermissionDto
{
    public int PermissionId { get; set; }
}

public class UserRolePermissionDto
{
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public int PermissionId { get; set; }
    public string? PermissionName { get; set; }
}
