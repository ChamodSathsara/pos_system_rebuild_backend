using AutoMapper;
using PosApi.DTOs.Security;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PermissionService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<PermissionDto>>(permissions);
    }

    public async Task<PermissionDto> GetByIdAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken)
            ?? throw new NotFoundException("Permission", permissionId);

        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task<PermissionDto> CreateAsync(CreatePermissionDto request, CancellationToken cancellationToken = default)
    {
        var permissionName = request.PermissionName.Trim();

        if (await _unitOfWork.Permissions.PermissionNameExistsAsync(permissionName, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"A permission named '{permissionName}' already exists.");
        }

        var permission = new Permission
        {
            PermissionName = permissionName,
            Description = request.Description
        };

        await _unitOfWork.Permissions.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionName} ({PermissionId}) created successfully", permission.PermissionName, permission.PermissionId);

        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task<PermissionDto> UpdateAsync(int permissionId, UpdatePermissionDto request, CancellationToken cancellationToken = default)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken)
            ?? throw new NotFoundException("Permission", permissionId);

        var permissionName = request.PermissionName.Trim();

        if (await _unitOfWork.Permissions.PermissionNameExistsAsync(permissionName, permissionId, cancellationToken))
        {
            throw new ConflictException($"A permission named '{permissionName}' already exists.");
        }

        permission.PermissionName = permissionName;
        permission.Description = request.Description;

        _unitOfWork.Permissions.Update(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} updated successfully", permission.PermissionId);

        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task DeleteAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId, cancellationToken)
            ?? throw new NotFoundException("Permission", permissionId);

        var isAssignedToRoles = await _unitOfWork.UserRolePermissions.ExistsForPermissionAsync(permissionId, cancellationToken);
        if (isAssignedToRoles)
        {
            throw new ConflictException($"Permission '{permissionId}' cannot be deleted while it is still assigned to one or more roles.");
        }

        _unitOfWork.Permissions.Remove(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission {PermissionId} deleted successfully", permissionId);
    }
}
