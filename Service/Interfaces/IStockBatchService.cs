using PosApi.DTOs.Stock;

namespace PosApi.Service.Interfaces;

public interface IStockBatchService
{
    Task<IReadOnlyList<StockBatchDto>> GetByStockIdAsync(int stockId, CancellationToken cancellationToken = default);

    Task<StockBatchDto> GetByIdAsync(long batchId, CancellationToken cancellationToken = default);

    /// <summary>Receives new stock into a batch: raises an "In" movement and increases the parent stock line's CurrentQty.</summary>
    Task<StockBatchDto> CreateAsync(CreateStockBatchDto request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates batch metadata. Transitioning Status to Expired/Damaged/Blocked automatically writes
    /// off any remaining AvailableQty via a generated adjustment movement.
    /// </summary>
    Task<StockBatchDto> UpdateAsync(long batchId, UpdateStockBatchDto request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>Deletes a batch. Only allowed while it is untouched (AvailableQty == ReceivedQty, no movements beyond the initial receipt).</summary>
    Task DeleteAsync(long batchId, CancellationToken cancellationToken = default);
}
