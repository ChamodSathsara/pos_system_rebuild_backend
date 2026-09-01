using AutoMapper;
using PosApi.DTOs.Security;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class UserRolePermissionService : IUserRolePermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserRolePermissionService> _logger;

    public UserRolePermissionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserRolePermissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserRolePermissionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var mappings = await _unitOfWork.UserRolePermissions.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<UserRolePermissionDto>>(mappings);
    }

    public async Task<IReadOnlyList<UserRolePermissionDto>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("UserRole", roleId);

        var mappings = await _unitOfWork.UserRolePermissions.GetByRoleIdAsync(roleId, cancellationToken);
        return _mapper.Map<IReadOnlyList<UserRolePermissionDto>>(mappings);
    }

    public async Task<UserRolePermissionDto> AssignAsync(int roleId, AssignPermissionDto request, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("UserRole", roleId);

        var permission = await _unitOfWork.Permissions.GetByIdAsync(request.PermissionId, cancellationToken)
            ?? throw new NotFoundException("Permission", request.PermissionId);

        if (await _unitOfWork.UserRolePermissions.ExistsAsync(roleId, request.PermissionId, cancellationToken))
        {
            throw new ConflictException($"Permission '{request.PermissionId}' is already assigned to role '{roleId}'.");
        }

        var mapping = new UserRolePermission
        {
            RoleId = roleId,
            PermissionId = request.PermissionId
        };

        await _unitOfWork.UserRolePermissions.AddAsync(mapping, cancellationToken);
        await _unitOfWork.UserRolePermissions.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} assigned to role {RoleId}", request.PermissionId, roleId);

        return new UserRolePermissionDto
        {
            RoleId = roleId,
            RoleName = role.RoleName,
            PermissionId = permission.PermissionId,
            PermissionName = permission.PermissionName
        };
    }

    public async Task RemoveAsync(int roleId, int permissionId, CancellationToken cancellationToken = default)
    {
        var mapping = await _unitOfWork.UserRolePermissions.GetAsync(roleId, permissionId, cancellationToken)
            ?? throw new NotFoundException($"Permission '{permissionId}' is not assigned to role '{roleId}'.");

        _unitOfWork.UserRolePermissions.Remove(mapping);
        await _unitOfWork.UserRolePermissions.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} removed from role {RoleId}", permissionId, roleId);
    }
}
