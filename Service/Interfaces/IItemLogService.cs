using PosApi.DTOs.Product;

namespace PosApi.Service.Interfaces;

public interface IItemLogService
{
    Task<IReadOnlyList<ItemLogDto>> SearchAsync(
        string? itemCode,
        string? action,
        string? changedBy,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<ItemLogDto> GetByIdAsync(int logId, CancellationToken cancellationToken = default);

    /// <summary>Records a new item change-log entry. ChangedBy is always set to the currently authenticated user.</summary>
    Task<ItemLogDto> CreateAsync(CreateItemLogDto request, string changedBy, CancellationToken cancellationToken = default);

    Task DeleteAsync(int logId, CancellationToken cancellationToken = default);
}
