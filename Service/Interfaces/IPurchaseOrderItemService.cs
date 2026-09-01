using PosApi.DTOs.Purchase;

namespace PosApi.Service.Interfaces;

public interface IPurchaseOrderItemService
{
    Task<IReadOnlyList<PurchaseOrderItemDto>> GetByPoNoAsync(string poNo, CancellationToken cancellationToken = default);

    Task<PurchaseOrderItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Adds a line item to an existing open purchase order and recomputes its TotalAmount.</summary>
    Task<PurchaseOrderItemDto> CreateAsync(CreatePurchaseOrderItemDto request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>Updates a line item's quantity/cost. Only allowed while the parent order is Open and nothing has been received against this line.</summary>
    Task<PurchaseOrderItemDto> UpdateAsync(int id, UpdatePurchaseOrderItemDto request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>Removes a line item. Only allowed while the parent order is Open, nothing has been received against it, and it is not the order's last remaining line.</summary>
    Task DeleteAsync(int id, string updatedBy, CancellationToken cancellationToken = default);
}