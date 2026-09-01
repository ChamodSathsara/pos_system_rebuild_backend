using PosApi.DTOs.Sale;

namespace PosApi.Service.Interfaces;

public interface ISaleReturnService
{
    Task<IReadOnlyList<SaleReturnDto>> SearchAsync(
        string? invoiceNo,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<SaleReturnDto> GetByIdAsync(string returnNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a return against a completed sale: inserts the header/lines and restores stock
    /// inventory/batches (crediting back the same batches the original sale drew from, FIFO,
    /// raising an IN stock movement per batch touched) - all in a single transaction.
    /// </summary>
    Task<SaleReturnDto> CreateAsync(CreateSaleReturnDto request, string createdBy, CancellationToken cancellationToken = default);
}
