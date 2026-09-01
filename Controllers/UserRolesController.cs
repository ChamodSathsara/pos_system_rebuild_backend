using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosApi.Common;
using PosApi.Constants;
using PosApi.DTOs.Security;
using PosApi.Service.Interfaces;

namespace PosApi.Controllers;

/// <summary>
/// User role management endpoints. Restricted to Admins since roles drive authorization
/// throughout the system.
/// </summary>
[ApiController]
[Route("api/user-roles")]
[Authorize(Roles = RoleConstants.Admin)]
public class UserRolesController : BaseApiController
{
    private readonly IUserRoleService _userRoleService;

    public UserRolesController(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    /// <summary>
    /// Retrieves every role.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserRoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _userRoleService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserRoleDto>>.SuccessResponse(roles));
    }

    /// <summary>
    /// Retrieves a single role by id.
    /// </summary>
    [HttpGet("{roleId:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int roleId, CancellationToken cancellationToken)
    {
        var role = await _userRoleService.GetByIdAsync(roleId, cancellationToken);
        return Ok(ApiResponse<UserRoleDto>.SuccessResponse(role));
    }

    /// <summary>
    /// Retrieves a role together with its assigned permissions.
    /// </summary>
    [HttpGet("{roleId:int}/details")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleWithPermissionsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdWithPermissions(int roleId, CancellationToken cancellationToken)
    {
        var role = await _userRoleService.GetByIdWithPermissionsAsync(roleId, cancellationToken);
        return Ok(ApiResponse<UserRoleWithPermissionsDto>.SuccessResponse(role));
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserRoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRoleDto request, CancellationToken cancellationToken)
    {
        var role = await _userRoleService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { roleId = role.RoleId },
            ApiResponse<UserRoleDto>.SuccessResponse(role, "Role created successfully."));
    }

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    [HttpPut("{roleId:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int roleId, [FromBody] UpdateUserRoleDto request, CancellationToken cancellationToken)
    {
        var role = await _userRoleService.UpdateAsync(roleId, request, cancellationToken);
        return Ok(ApiResponse<UserRoleDto>.SuccessResponse(role, "Role updated successfully."));
    }

    /// <summary>
    /// Deletes a role. Fails if it is still assigned to any system user.
    /// </summary>
    [HttpDelete("{roleId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int roleId, CancellationToken cancellationToken)
    {
        await _userRoleService.DeleteAsync(roleId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Role deleted successfully."));
    }
}
