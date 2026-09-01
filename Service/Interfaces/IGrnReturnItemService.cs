using PosApi.DTOs.Grn;

namespace PosApi.Service.Interfaces;

/// <summary>
/// Read-only access to GRN return line items. Lines are only ever created as part of posting a
/// GRN return (see IGrnReturnService.CreateAsync) since each one drives stock/batch/ledger side
/// effects - there is no standalone create/update here.
/// </summary>
public interface IGrnReturnItemService
{
    Task<IReadOnlyList<GrnReturnItemDto>> GetByGrnReturnIdAsync(int grnReturnId, CancellationToken cancellationToken = default);

    Task<GrnReturnItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
