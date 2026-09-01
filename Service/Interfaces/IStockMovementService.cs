using PosApi.DTOs.Stock;

namespace PosApi.Service.Interfaces;

public interface IStockMovementService
{
    Task<IReadOnlyList<StockMovementDto>> SearchAsync(
        int? stockId,
        long? batchId,
        string? referenceNo,
        CancellationToken cancellationToken = default);

    Task<StockMovementDto> GetByIdAsync(long movementId, CancellationToken cancellationToken = default);

    /// <summary>Records a manual movement against a batch (Qty is signed) and keeps the batch and stock line in sync.</summary>
    Task<StockMovementDto> CreateAsync(CreateStockMovementDto request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>Movements are an immutable audit trail: only ReferenceNo/Remarks can be corrected here.</summary>
    Task<StockMovementDto> UpdateAsync(long movementId, UpdateStockMovementDto request, CancellationToken cancellationToken = default);

    /// <summary>Reverses and deletes a movement. Only the most recently recorded Adjustment movement for a batch can be removed.</summary>
    Task DeleteAsync(long movementId, CancellationToken cancellationToken = default);
}
