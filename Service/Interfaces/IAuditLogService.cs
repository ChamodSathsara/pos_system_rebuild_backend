using PosApi.DTOs.Security;

namespace PosApi.Service.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogPageDto> SearchAsync(
        int pageNumber,
        int pageSize,
        string? userCode,
        string? action,
        string? tableName,
        string? recordId,
        string? branchCode,
        string? warehouseCode,
        Guid? transactionId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<AuditLogDto> GetByIdAsync(int logId, CancellationToken cancellationToken = default);
}
