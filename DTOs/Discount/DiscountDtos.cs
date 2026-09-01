using PosApi.Models.Enums;

namespace PosApi.DTOs.Discount;

/// <summary>
/// Fields required per DiscountType:
/// Item          - ItemCode required. StartDate/EndDate optional.
/// Item_Quantity - ItemCode + MinQuantity required. StartDate/EndDate optional.
/// Seasonal      - StartDate + EndDate required. ItemCode/MinQuantity/MinBillAmount must be omitted.
/// Total_Bill    - MinBillAmount required. ItemCode/MinQuantity/StartDate/EndDate must be omitted.
/// Special       - ItemCode + StartDate + EndDate required. MinQuantity/MinBillAmount must be omitted.
/// ApplicableTo is derived automatically from DiscountType and is not accepted from the client.
/// </summary>
public class CreateDiscountDto
{
    /// <summary>Optional. Auto-generated (e.g. DIS00001) when omitted.</summary>
    public string? DiscountCode { get; set; }
    public string DiscountName { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public DiscountMethod DiscountMethod { get; set; }
    public decimal DiscountValue { get; set; }
    public string? ItemCode { get; set; }
    public decimal? MinQuantity { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public decimal? MinBillAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateDiscountDto
{
    public string DiscountName { get; set; } = string.Empty;
    public DiscountMethod DiscountMethod { get; set; }
    public decimal DiscountValue { get; set; }
    public string? ItemCode { get; set; }
    public decimal? MinQuantity { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public decimal? MinBillAmount { get; set; }
    public bool IsActive { get; set; }
}

public class DiscountDto
{
    public string DiscountCode { get; set; } = string.Empty;
    public string DiscountName { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public DiscountMethod DiscountMethod { get; set; }
    public decimal? DiscountValue { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal? MinQuantity { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public decimal? MinBillAmount { get; set; }
    public DiscountApplicableTo ApplicableTo { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>Request to work out which discounts currently apply to a candidate sale line and/or bill total.</summary>
public class EvaluateDiscountRequestDto
{
    /// <summary>Item being sold. When supplied, item-level discounts (Item, Item_Quantity, Special) are evaluated.</summary>
    public string? ItemCode { get; set; }

    /// <summary>Quantity of ItemCode being purchased. Required to evaluate Item_Quantity discounts.</summary>
    public decimal? Quantity { get; set; }

    /// <summary>Line subtotal for ItemCode (Quantity * UnitPrice), used to calculate percentage-based item discounts.</summary>
    public decimal? ItemAmount { get; set; }

    /// <summary>Running bill total. When supplied, bill-level discounts (Seasonal, Total_Bill) are evaluated.</summary>
    public decimal? BillAmount { get; set; }

    /// <summary>Date to evaluate against. Defaults to today.</summary>
    public DateOnly? EvaluationDate { get; set; }

    /// <summary>Time to evaluate against. Only discounts with no time window, or a matching one, are considered.</summary>
    public TimeOnly? EvaluationTime { get; set; }
}

public class ApplicableDiscountDto
{
    public string DiscountCode { get; set; } = string.Empty;
    public string DiscountName { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public DiscountMethod DiscountMethod { get; set; }
    public decimal DiscountValue { get; set; }

    /// <summary>The discount amount calculated from the supplied ItemAmount/BillAmount and this discount's method/value.</summary>
    public decimal CalculatedAmount { get; set; }
}

public class DiscountEvaluationResultDto
{
    public IReadOnlyList<ApplicableDiscountDto> ItemLevelDiscounts { get; set; } = new List<ApplicableDiscountDto>();
    public IReadOnlyList<ApplicableDiscountDto> BillLevelDiscounts { get; set; } = new List<ApplicableDiscountDto>();
    public decimal TotalItemDiscount { get; set; }
    public decimal TotalBillDiscount { get; set; }
}
