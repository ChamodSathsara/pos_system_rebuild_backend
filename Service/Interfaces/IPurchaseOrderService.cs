using PosApi.DTOs.Purchase;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface IPurchaseOrderService
{
    Task<IReadOnlyList<PurchaseOrderDto>> SearchAsync(
        int? vendorId,
        string? branchCode,
        PurchaseOrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> GetByIdAsync(string poNo, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>Replaces an open purchase order's header/line items. Blocked once anything has been received against it.</summary>
    Task<PurchaseOrderDto> UpdateAsync(string poNo, UpdatePurchaseOrderDto request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>Records a manual Approved/Rejected review decision - a convenience wrapper that also feeds the PurchaseOrderHistory log.</summary>
    Task<PurchaseOrderDto> ApproveAsync(string poNo, string? remarks, string approvedBy, CancellationToken cancellationToken = default);

    Task<PurchaseOrderDto> RejectAsync(string poNo, string? remarks, string rejectedBy, CancellationToken cancellationToken = default);

    /// <summary>Cancels an order. Only allowed while Open and nothing has been received against it.</summary>
    Task<PurchaseOrderDto> CancelAsync(string poNo, CancelPurchaseOrderDto request, string cancelledBy, CancellationToken cancellationToken = default);

    /// <summary>Deletes an order. Only allowed while Open, with no GRNs and nothing received.</summary>
    Task DeleteAsync(string poNo, CancellationToken cancellationToken = default);
}