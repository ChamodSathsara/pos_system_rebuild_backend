using PosApi.DTOs.Stock;

namespace PosApi.Service.Interfaces;

public interface IStockTransferService
{
    Task<StockTransferRequestDto> CreateAsync(CreateStockTransferRequestDto request, string userCode, string? userBranchCode, string? role, CancellationToken ct = default);
    Task<IReadOnlyList<StockTransferRequestDto>> GetRequestsAsync(string? sourceWarehouseCode, string? destinationWarehouseCode, CancellationToken ct = default);
    Task<StockTransferRequestDto> GetRequestByIdAsync(long id, CancellationToken ct = default);
    Task<StockTransferRequestDto> AcceptAsync(long id, AcceptStockTransferRequestDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default);
    Task<StockTransferDispatchDto> DispatchAsync(long id, CreateStockTransferDispatchDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default);
    Task<StockTransferDispatchDto> DirectDispatchAsync(CreateDirectStockTransferDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default);
    Task<StockTransferRequestDto> CreateDirectProposalAsync(CreateDirectTransferProposalDto request, string userCode, string? userWarehouseCode, string? role, CancellationToken ct = default);
    Task<StockTransferRequestDto> BranchAcceptAsync(long id, BranchTransferDecisionDto request, string userCode, string? userBranchCode, string? role, CancellationToken ct = default);
    Task<StockTransferReceiptDto> ReceiveAsync(long dispatchId, ReceiveStockTransferDispatchDto request, string userCode, string? userBranchCode, string? role, CancellationToken ct = default);
    Task<(string FileName, byte[] Content)> GetDeliveryNotePdfAsync(long dispatchId, CancellationToken ct = default);
    Task<(string FileName, byte[] Content)> GetReceiptPdfAsync(long receiptId, CancellationToken ct = default);
}
