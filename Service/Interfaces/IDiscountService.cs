using PosApi.DTOs.Discount;
using PosApi.Models.Enums;

namespace PosApi.Service.Interfaces;

public interface IDiscountService
{
    Task<IReadOnlyList<DiscountDto>> GetAllAsync(
        DiscountType? discountType = null,
        bool? isActive = null,
        string? itemCode = null,
        CancellationToken cancellationToken = default);

    Task<DiscountDto> GetByCodeAsync(string discountCode, CancellationToken cancellationToken = default);

    Task<DiscountDto> CreateAsync(CreateDiscountDto request, string createdBy, CancellationToken cancellationToken = default);

    Task<DiscountDto> UpdateAsync(string discountCode, UpdateDiscountDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(string discountCode, CancellationToken cancellationToken = default);

    /// <summary>Works out which discounts currently apply for the given item/quantity/bill-amount and computes their amounts.</summary>
    Task<DiscountEvaluationResultDto> EvaluateAsync(EvaluateDiscountRequestDto request, CancellationToken cancellationToken = default);
}
