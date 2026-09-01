using AutoMapper;
using PosApi.DTOs.Security;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class UserRoleService : IUserRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserRoleService> _logger;

    public UserRoleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserRoleService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _unitOfWork.UserRoles.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<UserRoleDto>>(roles);
    }

    public async Task<UserRoleDto> GetByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("UserRole", roleId);

        return _mapper.Map<UserRoleDto>(role);
    }

    public async Task<UserRoleWithPermissionsDto> GetByIdWithPermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.UserRoles.GetByIdWithPermissionsAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("UserRole", roleId);

        return _mapper.Map<UserRoleWithPermissionsDto>(role);
    }

    public async Task<UserRoleDto> CreateAsync(CreateUserRoleDto request, CancellationToken cancellationToken = default)
    {
        var roleName = request.RoleName.Trim();

        if (await _unitOfWork.UserRoles.RoleNameExistsAsync(roleName, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"A role named '{roleName}' already exists.");
        }

        var role = new UserRole
        {
            RoleName = roleName,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserRoles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UserRole {RoleName} ({RoleId}) created successfully", role.RoleName, role.RoleId);

        return _mapper.Map<UserRoleDto>(role);
    }

    public async Task<UserRoleDto> UpdateAsync(int roleId, UpdateUserRoleDto request, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("UserRole", roleId);

        var roleName = request.RoleName.Trim();

        if (await _unitOfWork.UserRoles.RoleNameExistsAsync(roleName, roleId, cancellationToken))
        {
            throw new ConflictException($"A role named '{roleName}' already exists.");
        }

        role.RoleName = roleName;
        role.Description = request.Description;

        _unitOfWork.UserRoles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UserRole {RoleId} updated successfully", role.RoleId);

        return _mapper.Map<UserRoleDto>(role);
    }

    public async Task DeleteAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("UserRole", roleId);

        var isAssignedToUsers = await _unitOfWork.Users.ExistsAsync(u => u.RoleId == roleId, cancellationToken);
        if (isAssignedToUsers)
        {
            throw new ConflictException($"Role '{roleId}' cannot be deleted while it is still assigned to system users.");
        }

        _unitOfWork.UserRoles.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UserRole {RoleId} deleted successfully", roleId);
    }
}
