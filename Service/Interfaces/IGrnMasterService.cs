using PosApi.DTOs.Grn;

namespace PosApi.Service.Interfaces;

public interface IGrnMasterService
{
    Task<IReadOnlyList<GrnDto>> SearchAsync(
        string? poNo,
        int? vendorId,
        string? branchCode,
        string? warehouseCode,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<GrnDto> GetByIdAsync(int grnId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a GRN: inserts the header/lines, updates stock inventory and batches, raises a
    /// STOCK_IN movement, updates the source purchase order (items, status, history) and the
    /// vendor's ledger - all in a single transaction. See <see cref="CreateGrnDto"/> for details.
    /// </summary>
    Task<GrnDto> CreateAsync(CreateGrnDto request, string receivedBy, CancellationToken cancellationToken = default);

    /// <summary>Deletes a GRN. Only allowed while nothing has been returned against it; reverses every downstream effect (batches, movements, PO items/status/history, vendor ledger).</summary>
    Task DeleteAsync(int grnId, string updatedBy, CancellationToken cancellationToken = default);
}
