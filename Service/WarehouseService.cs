using AutoMapper;
using PosApi.DTOs.Organization;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<WarehouseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetAllAsync(string? branchCode = null, CancellationToken cancellationToken = default)
    {
        var warehouses = string.IsNullOrWhiteSpace(branchCode)
            ? await _unitOfWork.Warehouses.GetAllAsync(cancellationToken)
            : await _unitOfWork.Warehouses.GetByBranchCodeAsync(branchCode, cancellationToken);

        return _mapper.Map<IReadOnlyList<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseCode, cancellationToken)
            ?? throw new NotFoundException("Warehouse", warehouseCode);

        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto request, CancellationToken cancellationToken = default)
    {
        var warehouseCode = request.WarehouseCode.Trim();

        if (await _unitOfWork.Warehouses.WarehouseCodeExistsAsync(warehouseCode, cancellationToken))
        {
            throw new ConflictException($"A warehouse with code '{warehouseCode}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.BranchCode)
            && !await _unitOfWork.Branches.BranchCodeExistsAsync(request.BranchCode, cancellationToken))
        {
            throw new BadRequestException($"Branch '{request.BranchCode}' does not exist.");
        }

        var warehouse = new Warehouse
        {
            WarehouseCode = warehouseCode,
            WarehouseName = request.WarehouseName.Trim(),
            Address = request.Address,
            BranchCode = request.BranchCode,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Warehouses.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse {WarehouseCode} created successfully", warehouse.WarehouseCode);

        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(string warehouseCode, UpdateWarehouseDto request, CancellationToken cancellationToken = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseCode, cancellationToken)
            ?? throw new NotFoundException("Warehouse", warehouseCode);

        if (!string.IsNullOrWhiteSpace(request.BranchCode)
            && !await _unitOfWork.Branches.BranchCodeExistsAsync(request.BranchCode, cancellationToken))
        {
            throw new BadRequestException($"Branch '{request.BranchCode}' does not exist.");
        }

        warehouse.WarehouseName = request.WarehouseName.Trim();
        warehouse.Address = request.Address;
        warehouse.BranchCode = request.BranchCode;
        warehouse.IsActive = request.IsActive;

        _unitOfWork.Warehouses.Update(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse {WarehouseCode} updated successfully", warehouse.WarehouseCode);

        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task DeleteAsync(string warehouseCode, CancellationToken cancellationToken = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseCode, cancellationToken)
            ?? throw new NotFoundException("Warehouse", warehouseCode);

        _unitOfWork.Warehouses.Remove(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Warehouse {WarehouseCode} deleted successfully", warehouseCode);
    }
}
