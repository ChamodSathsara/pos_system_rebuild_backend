namespace PosApi.DTOs.Security;

public class AuditLogDto
{
    public int LogId { get; set; }
    public Guid? TransactionId { get; set; }
    public string? UserCode { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Action { get; set; }
    public string? TableName { get; set; }
    public string? RecordId { get; set; }
    public string? EntityType { get; set; }
    public string? BranchCode { get; set; }
    public string? WarehouseCode { get; set; }
    public string? ActionStatus { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public string? Metadata { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime? ActionTime { get; set; }
}

public class AuditLogPageDto
{
    public IReadOnlyList<AuditLogDto> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
