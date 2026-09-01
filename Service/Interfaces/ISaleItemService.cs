using PosApi.DTOs.Sale;

namespace PosApi.Service.Interfaces;

/// <summary>
/// Read-only access to sale line items. Lines are only ever created as part of posting a sale
/// (see ISaleService.CreateAsync) since each one drives stock side effects - there is no
/// standalone create/update here.
/// </summary>
public interface ISaleItemService
{
    Task<IReadOnlyList<SaleItemDto>> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default);

    Task<SaleItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
