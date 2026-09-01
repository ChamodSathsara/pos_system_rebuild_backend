using AutoMapper;
using PosApi.DTOs.Organization;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BranchService> _logger;

    public BranchService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BranchService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(string? companyCode = null, CancellationToken cancellationToken = default)
    {
        var branches = string.IsNullOrWhiteSpace(companyCode)
            ? await _unitOfWork.Branches.GetAllAsync(cancellationToken)
            : await _unitOfWork.Branches.GetByCompanyCodeAsync(companyCode, cancellationToken);

        return _mapper.Map<IReadOnlyList<BranchDto>>(branches);
    }

    public async Task<BranchDto> GetByCodeAsync(string branchCode, CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchCode, cancellationToken)
            ?? throw new NotFoundException("Branch", branchCode);

        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto request, CancellationToken cancellationToken = default)
    {
        var branchCode = request.BranchCode.Trim();

        if (await _unitOfWork.Branches.BranchCodeExistsAsync(branchCode, cancellationToken))
        {
            throw new ConflictException($"A branch with code '{branchCode}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.CompanyCode)
            && !await _unitOfWork.Companies.CompanyCodeExistsAsync(request.CompanyCode, cancellationToken))
        {
            throw new BadRequestException($"Company '{request.CompanyCode}' does not exist.");
        }

        var branch = new Branch
        {
            BranchCode = branchCode,
            BranchName = request.BranchName.Trim(),
            Address = request.Address,
            Phone = request.Phone,
            Status = request.Status,
            CompanyCode = request.CompanyCode,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Branches.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Branch {BranchCode} created successfully", branch.BranchCode);

        return _mapper.Map<BranchDto>(branch);
    }

    public async Task<BranchDto> UpdateAsync(string branchCode, UpdateBranchDto request, CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchCode, cancellationToken)
            ?? throw new NotFoundException("Branch", branchCode);

        if (!string.IsNullOrWhiteSpace(request.CompanyCode)
            && !await _unitOfWork.Companies.CompanyCodeExistsAsync(request.CompanyCode, cancellationToken))
        {
            throw new BadRequestException($"Company '{request.CompanyCode}' does not exist.");
        }

        branch.BranchName = request.BranchName.Trim();
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.Status = request.Status;
        branch.CompanyCode = request.CompanyCode;
        branch.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Branches.Update(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Branch {BranchCode} updated successfully", branch.BranchCode);

        return _mapper.Map<BranchDto>(branch);
    }

    public async Task DeleteAsync(string branchCode, CancellationToken cancellationToken = default)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchCode, cancellationToken)
            ?? throw new NotFoundException("Branch", branchCode);

        var hasWarehouses = await _unitOfWork.Warehouses.ExistsAsync(w => w.BranchCode == branchCode, cancellationToken);
        if (hasWarehouses)
        {
            throw new ConflictException($"Branch '{branchCode}' cannot be deleted while it still has warehouses assigned to it.");
        }

        var hasUsers = await _unitOfWork.Users.ExistsAsync(u => u.BranchCode == branchCode, cancellationToken);
        if (hasUsers)
        {
            throw new ConflictException($"Branch '{branchCode}' cannot be deleted while it still has system users assigned to it.");
        }

        _unitOfWork.Branches.Remove(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Branch {BranchCode} deleted successfully", branchCode);
    }
}
