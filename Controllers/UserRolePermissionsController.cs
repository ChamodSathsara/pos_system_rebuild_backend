using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.DTOs.Security;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// Manages which permissions are assigned to which role (the user_role_permission join table).
/// This is a pure many-to-many mapping, so "CRUD" here is expressed as Assign (create),
/// List (read), and Remove (delete) - there is no meaningful "update" for a join row.
/// Restricted to Admins.
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Roles = RoleConstants.Admin)]
public class UserRolePermissionsController : BaseApiController
{
    private readonly IUserRolePermissionService _userRolePermissionService;

    public UserRolePermissionsController(IUserRolePermissionService userRolePermissionService)
    {
        _userRolePermissionService = userRolePermissionService;
    }

    /// <summary>
    /// Retrieves every role-permission assignment in the system.
    /// </summary>
    [HttpGet("user-role-permissions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserRolePermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var mappings = await _userRolePermissionService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserRolePermissionDto>>.SuccessResponse(mappings));
    }

    /// <summary>
    /// Retrieves the permissions assigned to a specific role.
    /// </summary>
    [HttpGet("user-roles/{roleId:int}/permissions")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserRolePermissionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRole(int roleId, CancellationToken cancellationToken)
    {
        var mappings = await _userRolePermissionService.GetByRoleIdAsync(roleId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserRolePermissionDto>>.SuccessResponse(mappings));
    }

    /// <summary>
    /// Assigns an existing permission to a role.
    /// </summary>
    [HttpPost("user-roles/{roleId:int}/permissions")]
    [ProducesResponseType(typeof(ApiResponse<UserRolePermissionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Assign(int roleId, [FromBody] AssignPermissionDto request, CancellationToken cancellationToken)
    {
        var mapping = await _userRolePermissionService.AssignAsync(roleId, request, cancellationToken);

        return CreatedAtAction(
            nameof(GetByRole),
            new { roleId },
            ApiResponse<UserRolePermissionDto>.SuccessResponse(mapping, "Permission assigned to role successfully."));
    }

    /// <summary>
    /// Removes a permission from a role.
    /// </summary>
    [HttpDelete("user-roles/{roleId:int}/permissions/{permissionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int roleId, int permissionId, CancellationToken cancellationToken)
    {
        await _userRolePermissionService.RemoveAsync(roleId, permissionId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Permission removed from role successfully."));
    }
}
