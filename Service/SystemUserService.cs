using AutoMapper;
using PosApi.Constants;
using PosApi.DTOs.Security;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Security;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class SystemUserService : ISystemUserService
{
    private const string CodePrefix = "USR";
    private static readonly string[] BranchManagedRoles = [RoleConstants.Cashier, RoleConstants.BranchManager];

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<SystemUserService> _logger;

    public SystemUserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        ILogger<SystemUserService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SystemUserDto>> GetAllAsync(string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default)
    {
        var users = IsBranchManager(callerRole)
            ? await _unitOfWork.Users.GetBranchUsersWithRolesAsync(RequireBranch(callerBranchCode), BranchManagedRoles, cancellationToken)
            : await _unitOfWork.Users.GetAllWithRoleAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SystemUserDto>>(users);
    }

    public async Task<SystemUserDto> GetByCodeAsync(string userCode, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUserCodeWithRoleAsync(userCode, cancellationToken)
            ?? throw new NotFoundException("SystemUser", userCode);

        EnsureBranchManagerCanManage(user, callerRole, callerBranchCode);

        return _mapper.Map<SystemUserDto>(user);
    }

    public async Task<SystemUserDto> CreateAsync(CreateSystemUserDto request, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default)
    {
        if (IsBranchManager(callerRole))
        {
            var branchCode = RequireBranch(callerBranchCode);
            if (!string.IsNullOrWhiteSpace(request.BranchCode)
                && !string.Equals(request.BranchCode, branchCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenAppException("You can only create users for your assigned branch.");
            }

            await EnsureBranchManagedRoleAsync(request.RoleId, cancellationToken);
            request.BranchCode = branchCode;
        }

        var userCode = request.UserCode?.Trim();

        if (string.IsNullOrWhiteSpace(userCode))
        {
            userCode = await GenerateNextUserCodeAsync(cancellationToken);
        }
        else if (await _unitOfWork.Users.ExistsAsync(u => u.UserCode == userCode, cancellationToken))
        {
            throw new ConflictException($"A system user with code '{userCode}' already exists.");
        }

        if (await _unitOfWork.Users.UsernameExistsAsync(request.Username.Trim(), cancellationToken))
        {
            throw new ConflictException($"Username '{request.Username}' is already taken.");
        }

        if (!string.IsNullOrWhiteSpace(request.BranchCode)
            && !await _unitOfWork.Branches.BranchCodeExistsAsync(request.BranchCode, cancellationToken))
        {
            throw new BadRequestException($"Branch '{request.BranchCode}' does not exist.");
        }

        if (!string.IsNullOrWhiteSpace(request.WarehouseCode)
            && !await _unitOfWork.Warehouses.WarehouseCodeExistsAsync(request.WarehouseCode, cancellationToken))
        {
            throw new BadRequestException($"Warehouse '{request.WarehouseCode}' does not exist.");
        }

        if (request.RoleId.HasValue
            && await _unitOfWork.UserRoles.GetByIdAsync(request.RoleId.Value, cancellationToken) is null)
        {
            throw new BadRequestException($"Role '{request.RoleId}' does not exist.");
        }

        var user = new SystemUser
        {
            UserCode = userCode,
            Username = request.Username.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Mobile = request.Mobile,
            BranchCode = request.BranchCode,
            WarehouseCode = request.WarehouseCode,
            RoleId = request.RoleId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SystemUser {UserCode} created successfully", user.UserCode);

        var created = await _unitOfWork.Users.GetByUserCodeWithRoleAsync(user.UserCode, cancellationToken);
        return _mapper.Map<SystemUserDto>(created);
    }

    public async Task<SystemUserDto> UpdateAsync(string userCode, UpdateSystemUserDto request, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userCode, cancellationToken)
            ?? throw new NotFoundException("SystemUser", userCode);

        if (IsBranchManager(callerRole))
        {
            var existing = await _unitOfWork.Users.GetByUserCodeWithRoleAsync(userCode, cancellationToken)
                ?? throw new NotFoundException("SystemUser", userCode);
            EnsureBranchManagerCanManage(existing, callerRole, callerBranchCode);
            await EnsureBranchManagedRoleAsync(request.RoleId, cancellationToken);

            var branchCode = RequireBranch(callerBranchCode);
            if (!string.IsNullOrWhiteSpace(request.BranchCode)
                && !string.Equals(request.BranchCode, branchCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new ForbiddenAppException("You can only assign users to your own branch.");
            }
            request.BranchCode = branchCode;
        }

        if (!string.IsNullOrWhiteSpace(request.BranchCode)
            && !await _unitOfWork.Branches.BranchCodeExistsAsync(request.BranchCode, cancellationToken))
        {
            throw new BadRequestException($"Branch '{request.BranchCode}' does not exist.");
        }

        if (!string.IsNullOrWhiteSpace(request.WarehouseCode)
            && !await _unitOfWork.Warehouses.WarehouseCodeExistsAsync(request.WarehouseCode, cancellationToken))
        {
            throw new BadRequestException($"Warehouse '{request.WarehouseCode}' does not exist.");
        }

        if (request.RoleId.HasValue
            && await _unitOfWork.UserRoles.GetByIdAsync(request.RoleId.Value, cancellationToken) is null)
        {
            throw new BadRequestException($"Role '{request.RoleId}' does not exist.");
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Mobile = request.Mobile;
        user.BranchCode = request.BranchCode;
        user.WarehouseCode = request.WarehouseCode;
        user.RoleId = request.RoleId;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SystemUser {UserCode} updated successfully", user.UserCode);

        var updated = await _unitOfWork.Users.GetByUserCodeWithRoleAsync(user.UserCode, cancellationToken);
        return _mapper.Map<SystemUserDto>(updated);
    }

    public async Task DeleteAsync(string userCode, string? callerRole, string? callerBranchCode, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUserCodeWithRoleAsync(userCode, cancellationToken)
            ?? throw new NotFoundException("SystemUser", userCode);

        EnsureBranchManagerCanManage(user, callerRole, callerBranchCode);

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SystemUser {UserCode} deleted successfully", userCode);
    }

    private async Task<string> GenerateNextUserCodeAsync(CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.UserCode.StartsWith(CodePrefix), cancellationToken);

        var nextSequence = users
            .Select(u => u.UserCode.Length > CodePrefix.Length && int.TryParse(u.UserCode[CodePrefix.Length..], out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{CodePrefix}{nextSequence:D5}";
    }

    private static bool IsBranchManager(string? role) =>
        string.Equals(role, RoleConstants.BranchManager, StringComparison.OrdinalIgnoreCase);

    private static string RequireBranch(string? branchCode) =>
        !string.IsNullOrWhiteSpace(branchCode)
            ? branchCode
            : throw new ForbiddenAppException("Your Branch Manager account is not assigned to a branch.");

    private static void EnsureBranchManagerCanManage(SystemUser user, string? callerRole, string? callerBranchCode)
    {
        if (!IsBranchManager(callerRole)) return;

        var branchCode = RequireBranch(callerBranchCode);
        if (!string.Equals(user.BranchCode, branchCode, StringComparison.OrdinalIgnoreCase)
            || user.Role is null
            || !BranchManagedRoles.Contains(user.Role.RoleName, StringComparer.OrdinalIgnoreCase))
        {
            throw new ForbiddenAppException("You can only manage Cashier and Branch Manager users in your assigned branch.");
        }
    }

    private async Task EnsureBranchManagedRoleAsync(int? roleId, CancellationToken cancellationToken)
    {
        if (!roleId.HasValue)
        {
            throw new BadRequestException("Role is required. Branch Managers can assign only Cashier or Branch_Manager roles.");
        }

        var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId.Value, cancellationToken);
        if (role is null || role.RoleId is not (2 or 3)
            || !BranchManagedRoles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
        {
            throw new ForbiddenAppException("Branch Managers can assign only Cashier or Branch_Manager roles.");
        }
    }
}
