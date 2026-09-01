using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface IDamageItemRepository : IGenericRepository<DamageItem, int>
{
    Task<DamageItem?> GetByIdWithDetailsAsync(int damageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DamageItem>> SearchAsync(
        string? itemCode,
        string? branchCode,
        string? warehouseCode,
        DamageItemStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);
}
