using PosApi.DTOs.Purchase;

namespace PosApi.Service.Interfaces;

public interface IPurchaseOrderHistoryService
{
    Task<IReadOnlyList<PurchaseOrderHistoryDto>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default);

    Task<PurchaseOrderHistoryDto> GetByIdAsync(int historyId, CancellationToken cancellationToken = default);

    /// <summary>Manually records an Approved/Rejected review note. Other lifecycle actions are generated automatically.</summary>
    Task<PurchaseOrderHistoryDto> CreateAsync(CreatePurchaseOrderHistoryDto request, string changedBy, CancellationToken cancellationToken = default);

    /// <summary>History entries are an audit trail: only Remarks can be corrected here.</summary>
    Task<PurchaseOrderHistoryDto> UpdateAsync(int historyId, UpdatePurchaseOrderHistoryDto request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a manual review note. System-generated lifecycle entries (Created, Modified, Cancelled, StatusChanged) cannot be deleted.</summary>
    Task DeleteAsync(int historyId, CancellationToken cancellationToken = default);
}