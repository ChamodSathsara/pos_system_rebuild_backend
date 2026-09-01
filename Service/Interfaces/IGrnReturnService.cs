using PosApi.DTOs.Grn;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface IGrnReturnService
{
    Task<IReadOnlyList<GrnReturnDto>> SearchAsync(
        int? grnId,
        GrnReturnStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<GrnReturnDto> GetByIdAsync(int grnReturnId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a GRN return: inserts the header/lines, draws down stock inventory and the matching
    /// batch, raises a STOCK_OUT movement, rolls back the source purchase order (items, status,
    /// history) and the vendor's ledger - all in a single transaction. See
    /// <see cref="CreateGrnReturnDto"/> for details.
    /// </summary>
    Task<GrnReturnDto> CreateAsync(CreateGrnReturnDto request, string returnBy, CancellationToken cancellationToken = default);
}
