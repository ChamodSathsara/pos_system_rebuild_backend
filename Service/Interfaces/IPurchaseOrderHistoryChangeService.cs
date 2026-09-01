using PosApi.DTOs.Purchase;

namespace PosApi.Service.Interfaces;

public interface IPurchaseOrderHistoryChangeService
{
    Task<IReadOnlyList<PurchaseOrderHistoryChangeDto>> GetByHistoryIdAsync(int historyId, CancellationToken cancellationToken = default);

    Task<PurchaseOrderHistoryChangeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PurchaseOrderHistoryChangeDto> CreateAsync(CreatePurchaseOrderHistoryChangeDto request, CancellationToken cancellationToken = default);

    Task<PurchaseOrderHistoryChangeDto> UpdateAsync(int id, UpdatePurchaseOrderHistoryChangeDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}