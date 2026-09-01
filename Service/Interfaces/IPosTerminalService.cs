using PosApi.DTOs.Pos;

namespace PosApi.Service.Interfaces;

public interface IPosTerminalService
{
    Task<IReadOnlyList<PosTerminalItemDto>> GetItemsAsync(
        string branchCode,
        string? warehouseCode,
        int? categoryId,
        string? keyword,
        bool onlyAvailable,
        CancellationToken cancellationToken = default);
}