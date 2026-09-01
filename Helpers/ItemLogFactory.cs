using PosApi.Models.Entities;

namespace PosApi.Helpers;

/// <summary>
/// Builds ItemLog entities for the automatic audit-trail entries that get raised whenever a
/// product is updated, its price changes, stock changes, or a damage item is posted (see
/// Constants.ItemLogActions). Callers add the returned entity via
/// _unitOfWork.ItemLogs.AddAsync(...) alongside their own changes so it commits atomically with
/// the rest of the operation on the caller's single SaveChangesAsync call.
/// </summary>
public static class ItemLogFactory
{
    public static ItemLog Create(string itemCode, string action, string? oldValue, string? newValue, string changedBy)
    {
        return new ItemLog
        {
            ItemCode = itemCode,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        };
    }
}
