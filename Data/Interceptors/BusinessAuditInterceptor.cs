using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PosApi.Models.Entities;

namespace PosApi.Data.Interceptors;

/// <summary>
/// Writes business and administration audit records in the same SaveChanges transaction.
/// High-volume POS sales, payments and sale returns (including their stock changes) are excluded.
/// </summary>
public sealed class BusinessAuditInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private static readonly HashSet<string> AuditedEntities = new(StringComparer.Ordinal)
    {
        nameof(Company), nameof(Branch), nameof(Warehouse),
        nameof(UserRole), nameof(Permission), nameof(UserRolePermission), nameof(SystemUser),
        nameof(Vendor), nameof(Category), nameof(Brand), nameof(TaxMaster), nameof(ProductMaster), nameof(Discount),
        nameof(PurchaseOrder), nameof(PurchaseOrderItem),
        nameof(GrnMaster), nameof(GrnItem), nameof(GrnReturn), nameof(GrnReturnItem),
        nameof(StockInventory), nameof(StockBatch), nameof(StockMovement), nameof(DamageItem),
        nameof(StockTransferRequest), nameof(StockTransferRequestLine), nameof(StockTransferDispatch),
        nameof(StockTransferDispatchLine), nameof(StockTransferReceipt), nameof(StockTransferReceiptLine),
        nameof(CentralStockReceipt), nameof(CentralStockReceiptLine),
        nameof(Expense), nameof(ExpenseCategory)
    };

    private static readonly HashSet<string> PosEntities = new(StringComparer.Ordinal)
    {
        nameof(Sale), nameof(SaleItem), nameof(SaleReturn), nameof(SaleReturnItem), nameof(Payment)
    };

    private static readonly string[] SensitiveFragments =
    {
        "password", "token", "secret", "privatekey", "certificate"
    };

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditRows(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditRows(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditRows(DbContext? context)
    {
        if (context is null || context.ChangeTracker.Entries<AuditLog>().Any()) return;

        context.ChangeTracker.DetectChanges();
        var entries = context.ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
        var isPosOperation = entries.Any(x => PosEntities.Contains(x.Metadata.ClrType.Name));
        var transactionId = Guid.NewGuid();
        var http = httpContextAccessor.HttpContext;
        var userCode = http?.User.FindFirstValue("user_id")
            ?? http?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "SYSTEM";

        var auditRows = entries
            .Where(x => ShouldAudit(x, isPosOperation))
            .Select(x => BuildAudit(x, transactionId, userCode, http))
            .ToList();

        if (auditRows.Count > 0) context.Set<AuditLog>().AddRange(auditRows);
    }

    private static bool ShouldAudit(EntityEntry entry, bool isPosOperation)
    {
        var entityName = entry.Metadata.ClrType.Name;
        if (!AuditedEntities.Contains(entityName)) return false;
        if (isPosOperation && entityName is nameof(StockInventory) or nameof(StockBatch) or nameof(StockMovement)) return false;
        if (entityName == nameof(SystemUser) && entry.State == EntityState.Modified &&
            entry.Properties.Where(x => x.IsModified).All(x => x.Metadata.Name == nameof(SystemUser.LastLogin))) return false;
        return entry.State != EntityState.Modified || entry.Properties.Any(x => x.IsModified);
    }

    private static AuditLog BuildAudit(EntityEntry entry, Guid transactionId, string userCode, HttpContext? http)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        foreach (var property in entry.Properties.Where(x => !IsSensitive(x.Metadata.Name)))
        {
            if (entry.State == EntityState.Added) newValues[property.Metadata.Name] = property.CurrentValue;
            else if (entry.State == EntityState.Deleted) oldValues[property.Metadata.Name] = property.OriginalValue;
            else if (property.IsModified)
            {
                oldValues[property.Metadata.Name] = property.OriginalValue;
                newValues[property.Metadata.Name] = property.CurrentValue;
            }
        }

        return new AuditLog
        {
            TransactionId = transactionId,
            UserCode = userCode,
            Action = entry.State switch { EntityState.Added => "CREATE", EntityState.Deleted => "DELETE", _ => "UPDATE" },
            TableName = entry.Metadata.GetTableName() ?? entry.Metadata.ClrType.Name,
            RecordId = GetRecordId(entry),
            EntityType = entry.Metadata.ClrType.Name,
            BranchCode = GetString(entry, "BranchCode"),
            WarehouseCode = GetString(entry, "WarehouseCode")
                ?? GetString(entry, "SourceWarehouseCode")
                ?? GetString(entry, "DestinationWarehouseCode"),
            ActionStatus = "Success",
            Reason = GetString(entry, "Reason") ?? GetString(entry, "ReasonDescription") ?? GetString(entry, "Remarks"),
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = http?.TraceIdentifier,
            Metadata = JsonSerializer.Serialize(new
            {
                method = http?.Request.Method,
                path = http?.Request.Path.Value,
                changedProperties = entry.State == EntityState.Modified
                    ? entry.Properties.Where(x => x.IsModified).Select(x => x.Metadata.Name).ToArray()
                    : entry.Properties.Select(x => x.Metadata.Name).ToArray()
            }),
            OldValue = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues),
            NewValue = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues),
            ActionTime = DateTime.UtcNow
        };
    }

    private static string GetRecordId(EntityEntry entry)
    {
        var keys = entry.Properties.Where(x => x.Metadata.IsPrimaryKey()).ToList();
        if (keys.Count == 0) return "unknown";
        return string.Join("|", keys.Select(x => x.IsTemporary ? $"{x.Metadata.Name}=pending" : $"{x.Metadata.Name}={x.CurrentValue ?? x.OriginalValue}"));
    }

    private static string? GetString(EntityEntry entry, string propertyName)
    {
        var property = entry.Properties.FirstOrDefault(x => x.Metadata.Name == propertyName);
        return property?.CurrentValue?.ToString() ?? property?.OriginalValue?.ToString();
    }

    private static bool IsSensitive(string propertyName) =>
        SensitiveFragments.Any(x => propertyName.Replace("_", string.Empty).Contains(x, StringComparison.OrdinalIgnoreCase));
}
