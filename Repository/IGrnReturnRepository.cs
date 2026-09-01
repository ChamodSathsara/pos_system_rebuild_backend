using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface IGrnReturnRepository : IGenericRepository<GrnReturn, int>
{
    Task<GrnReturn?> GetByIdWithDetailsAsync(int grnReturnId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GrnReturn>> SearchAsync(
        int? grnId,
        GrnReturnStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    /// <summary>Sums the quantity already returned across all GrnReturnItems raised against a given GrnItem, so a new return can be checked against what's still returnable.</summary>
    Task<decimal> GetReturnedQuantityForGrnItemAsync(int grnItemId, CancellationToken cancellationToken = default);
}
