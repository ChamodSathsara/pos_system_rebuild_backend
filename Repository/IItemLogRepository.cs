using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IItemLogRepository : IGenericRepository<ItemLog, int>
{
    Task<ItemLog?> GetByIdWithDetailsAsync(int logId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemLog>> SearchAsync(
        string? itemCode,
        string? action,
        string? changedBy,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);
}
