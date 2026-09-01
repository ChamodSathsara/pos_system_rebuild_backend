using PosApi.DTOs.Organization;

namespace PosApi.Service.Interfaces;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAllAsync(string? branchCode = null, CancellationToken cancellationToken = default);
    Task<WarehouseDto> GetByCodeAsync(string warehouseCode, CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto request, CancellationToken cancellationToken = default);
    Task<WarehouseDto> UpdateAsync(string warehouseCode, UpdateWarehouseDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string warehouseCode, CancellationToken cancellationToken = default);
}
