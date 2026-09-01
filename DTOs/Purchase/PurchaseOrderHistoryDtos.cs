using PosApi.Models.Enums;

namespace PosApi.DTOs.Purchase;

/// <summary>
/// Manually records a review decision against a purchase order. Lifecycle entries (Created,
/// Modified, Cancelled, StatusChanged) are generated automatically by the corresponding
/// PurchaseOrder actions and cannot be created here - only Approved/Rejected review notes can.
/// </summary>
public class CreatePurchaseOrderHistoryDto
{
    public string PoNo { get; set; } = string.Empty;
    public PurchaseOrderHistoryAction Action { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>History entries are an audit trail: only the descriptive Remarks can be corrected here.</summary>
public class UpdatePurchaseOrderHistoryDto
{
    public string? Remarks { get; set; }
}

public class PurchaseOrderHistoryDto
{
    public int HistoryId { get; set; }
    public string? PoNo { get; set; }
    public PurchaseOrderHistoryAction Action { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime? ChangedAt { get; set; }
    public string? Remarks { get; set; }
    public List<PurchaseOrderHistoryChangeDto> Changes { get; set; } = new();
}