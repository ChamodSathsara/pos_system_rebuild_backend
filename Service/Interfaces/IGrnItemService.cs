using PosApi.DTOs.Grn;

namespace PosApi.Service.Interfaces;

/// <summary>
/// Read-only access to GRN line items. Lines are only ever created as part of posting a GRN
/// (see IGrnMasterService.CreateAsync) since each one drives stock/batch/ledger side effects -
/// there is no standalone create/update here.
/// </summary>
public interface IGrnItemService
{
    Task<IReadOnlyList<GrnItemDto>> GetByGrnIdAsync(int grnId, CancellationToken cancellationToken = default);

    Task<GrnItemDto> GetByIdAsync(int grnItemId, CancellationToken cancellationToken = default);
}
