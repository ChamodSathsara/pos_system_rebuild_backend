using PosApi.DTOs.Sale;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface ISaleService
{
    /// <summary>
    /// Builds the printable invoice/receipt for a completed sale: letterhead (company/branch),
    /// cashier, customer, priced line items (original Price, discounted "Last Price", and net
    /// Amount), bill totals, and the payments recorded against it. Reuses the same sale
    /// aggregate as <see cref="GetByIdAsync"/> - no separate invoice storage or duplicated
    /// sale/payment logic.
    /// </summary>
    Task<SaleInvoiceDto> GetInvoiceAsync(string invoiceNo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleDto>> SearchAsync(
        string? branchCode,
        string? customerCode,
        SaleStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<SaleDto> GetByIdAsync(string invoiceNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a completed sale with its line items. Draws down stock FIFO across the branch's
    /// available batches for every line, computes tax from each product's tax code, and - if any
    /// initial payments are supplied - records them against the new invoice as well.
    /// </summary>
    Task<SaleDto> CreateAsync(CreateSaleDto request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Voids a completed sale, restoring the stock it drew down to the same batches it was taken
    /// from and marking it Cancelled. Blocked once any returns or payments have been recorded
    /// against the invoice.
    /// </summary>
    Task<SaleDto> CancelAsync(string invoiceNo, string cancelledBy, CancellationToken cancellationToken = default);
}
