using PosApi.Models.Entities;
using PosApi.Models.Enums;

namespace PosApi.Repository;

public interface IDiscountRepository : IGenericRepository<Discount, string>
{
    Task<bool> DiscountCodeExistsAsync(string discountCode, CancellationToken cancellationToken = default);

    /// <summary>Returns the next sequential discount code (e.g. "DIS00001", "DIS00002", ...) for use when the caller does not supply one explicitly.</summary>
    Task<string> GenerateNextDiscountCodeAsync(CancellationToken cancellationToken = default);

    Task<Discount?> GetByCodeWithDetailsAsync(string discountCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Discount>> SearchAsync(
        DiscountType? discountType,
        bool? isActive,
        string? itemCode,
        CancellationToken cancellationToken = default);

    /// <summary>Active, item-scoped discounts (Item, Item_Quantity, Special) for the given item whose date window covers <paramref name="date"/>.</summary>
    Task<IReadOnlyList<Discount>> GetActiveItemDiscountsAsync(string itemCode, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>Active, bill-scoped discounts (Seasonal, Total_Bill) whose date window covers <paramref name="date"/>.</summary>
    Task<IReadOnlyList<Discount>> GetActiveBillDiscountsAsync(DateOnly date, CancellationToken cancellationToken = default);
}
