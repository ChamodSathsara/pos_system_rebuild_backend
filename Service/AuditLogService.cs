using Microsoft.EntityFrameworkCore;
using PosApi.Data;
using PosApi.DTOs.Security;
using PosApi.Exceptions;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class AuditLogService(ApplicationDbContext context) : IAuditLogService
{
    public async Task<AuditLogPageDto> SearchAsync(
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
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(userCode)) query = query.Where(x => x.UserCode == userCode.Trim());
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action == action.Trim());
        if (!string.IsNullOrWhiteSpace(tableName)) query = query.Where(x => x.TableName == tableName.Trim());
        if (!string.IsNullOrWhiteSpace(recordId)) query = query.Where(x => x.RecordId == recordId.Trim());
        if (!string.IsNullOrWhiteSpace(branchCode)) query = query.Where(x => x.BranchCode == branchCode.Trim());
        if (!string.IsNullOrWhiteSpace(warehouseCode)) query = query.Where(x => x.WarehouseCode == warehouseCode.Trim());
        if (transactionId.HasValue) query = query.Where(x => x.TransactionId == transactionId.Value);
        if (fromDate.HasValue) query = query.Where(x => x.ActionTime >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(x => x.ActionTime < toDate.Value.Date.AddDays(1));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await (
            from log in query
            join user in context.SystemUsers.AsNoTracking() on log.UserCode equals user.UserCode into users
            from user in users.DefaultIfEmpty()
            orderby log.ActionTime descending, log.LogId descending
            select new AuditLogDto
            {
                LogId = log.LogId,
                TransactionId = log.TransactionId,
                UserCode = log.UserCode,
                Username = user == null ? null : user.Username,
                FullName = user == null ? null : user.FullName,
                Action = log.Action,
                TableName = log.TableName,
                RecordId = log.RecordId,
                EntityType = log.EntityType,
                BranchCode = log.BranchCode,
                WarehouseCode = log.WarehouseCode,
                ActionStatus = log.ActionStatus,
                Reason = log.Reason,
                IpAddress = log.IpAddress,
                CorrelationId = log.CorrelationId,
                Metadata = log.Metadata,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                ActionTime = log.ActionTime
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new AuditLogPageDto
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AuditLogDto> GetByIdAsync(int logId, CancellationToken cancellationToken = default)
    {
        var result = await (
            from log in context.AuditLogs.AsNoTracking()
            join user in context.SystemUsers.AsNoTracking() on log.UserCode equals user.UserCode into users
            from user in users.DefaultIfEmpty()
            where log.LogId == logId
            select new AuditLogDto
            {
                LogId = log.LogId,
                TransactionId = log.TransactionId,
                UserCode = log.UserCode,
                Username = user == null ? null : user.Username,
                FullName = user == null ? null : user.FullName,
                Action = log.Action,
                TableName = log.TableName,
                RecordId = log.RecordId,
                EntityType = log.EntityType,
                BranchCode = log.BranchCode,
                WarehouseCode = log.WarehouseCode,
                ActionStatus = log.ActionStatus,
                Reason = log.Reason,
                IpAddress = log.IpAddress,
                CorrelationId = log.CorrelationId,
                Metadata = log.Metadata,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                ActionTime = log.ActionTime
            }).SingleOrDefaultAsync(cancellationToken);

        return result ?? throw new NotFoundException("AuditLog", logId);
    }
}
