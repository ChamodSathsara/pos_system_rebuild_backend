using PosApi.DTOs.Sale;

namespace PosApi.Service.Interfaces;

/// <summary>
/// Read-only access to sale return line items. Lines are only ever created as part of posting a
/// sale return (see ISaleReturnService.CreateAsync) since each one drives stock side effects -
/// there is no standalone create/update here.
/// </summary>
public interface ISaleReturnItemService
{
    Task<IReadOnlyList<SaleReturnItemDto>> GetByReturnNoAsync(string returnNo, CancellationToken cancellationToken = default);

    Task<SaleReturnItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
