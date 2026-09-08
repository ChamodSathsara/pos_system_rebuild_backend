using PosApi.DTOs.Stock;

namespace PosApi.Service.Interfaces;

public interface IOpeningStockService
{
    Task<OpeningStockDto> CreateAsync(
        CreateOpeningStockDto request,
        string createdBy,
        string? userBranchCode,
        string? userWarehouseCode,
        string? userRole,
        CancellationToken cancellationToken = default);
}
