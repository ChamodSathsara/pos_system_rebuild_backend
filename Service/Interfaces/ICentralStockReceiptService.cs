using PosApi.DTOs.Stock;

namespace PosApi.Service.Interfaces;

public interface ICentralStockReceiptService
{
    Task<IReadOnlyList<CentralStockReceiptDto>> SearchAsync(string? warehouseCode, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<CentralStockReceiptDto> GetByIdAsync(long receiptId, CancellationToken ct = default);
    Task<CentralStockReceiptDto> CreateAsync(CreateCentralStockReceiptDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default);
    Task<(string FileName, byte[] Content)> GetReceiptPdfAsync(long receiptId, CancellationToken ct = default);
}
