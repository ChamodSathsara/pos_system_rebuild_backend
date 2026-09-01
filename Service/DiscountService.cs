using AutoMapper;
using PosApi.DTOs.Discount;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Models.Enums;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class DiscountService : IDiscountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DiscountService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DiscountDto>> GetAllAsync(
        DiscountType? discountType = null,
        bool? isActive = null,
        string? itemCode = null,
        CancellationToken cancellationToken = default)
    {
        var discounts = await _unitOfWork.Discounts.SearchAsync(discountType, isActive, itemCode, cancellationToken);
        return _mapper.Map<IReadOnlyList<DiscountDto>>(discounts);
    }

    public async Task<DiscountDto> GetByCodeAsync(string discountCode, CancellationToken cancellationToken = default)
    {
        var discount = await _unitOfWork.Discounts.GetByCodeWithDetailsAsync(discountCode, cancellationToken)
            ?? throw new NotFoundException("Discount", discountCode);

        return _mapper.Map<DiscountDto>(discount);
    }

    public async Task<DiscountDto> CreateAsync(CreateDiscountDto request, string createdBy, CancellationToken cancellationToken = default)
    {
        var discountCode = string.IsNullOrWhiteSpace(request.DiscountCode)
            ? await _unitOfWork.Discounts.GenerateNextDiscountCodeAsync(cancellationToken)
            : request.DiscountCode.Trim();

        if (await _unitOfWork.Discounts.DiscountCodeExistsAsync(discountCode, cancellationToken))
        {
            throw new ConflictException($"A discount with code '{discountCode}' already exists.");
        }

        var itemCode = NormalizeItemCode(request.ItemCode);

        if (itemCode is not null && !await _unitOfWork.Products.ExistsAsync(p => p.ItemCode == itemCode, cancellationToken))
        {
            throw new NotFoundException("Product", itemCode);
        }

        var discount = new Discount
        {
            DiscountCode = discountCode,
            DiscountName = request.DiscountName.Trim(),
            DiscountType = request.DiscountType,
            DiscountMethod = request.DiscountMethod,
            DiscountValue = request.DiscountValue,
            ItemCode = itemCode,
            MinQuantity = request.MinQuantity,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MinBillAmount = request.MinBillAmount,
            ApplicableTo = ResolveApplicableTo(request.DiscountType),
            IsActive = request.IsActive,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Discounts.AddAsync(discount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Discount {DiscountCode} created successfully", discount.DiscountCode);

        return _mapper.Map<DiscountDto>(discount);
    }

    public async Task<DiscountDto> UpdateAsync(string discountCode, UpdateDiscountDto request, CancellationToken cancellationToken = default)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountCode, cancellationToken)
            ?? throw new NotFoundException("Discount", discountCode);

        var itemCode = NormalizeItemCode(request.ItemCode);

        if (itemCode is not null && !await _unitOfWork.Products.ExistsAsync(p => p.ItemCode == itemCode, cancellationToken))
        {
            throw new NotFoundException("Product", itemCode);
        }

        // DiscountType (and therefore ApplicableTo) is immutable once created - it defines which
        // fields are meaningful for this discount. Only the fields below can change.
        discount.DiscountName = request.DiscountName.Trim();
        discount.DiscountMethod = request.DiscountMethod;
        discount.DiscountValue = request.DiscountValue;
        discount.ItemCode = itemCode;
        discount.MinQuantity = request.MinQuantity;
        discount.StartDate = request.StartDate;
        discount.EndDate = request.EndDate;
        discount.StartTime = request.StartTime;
        discount.EndTime = request.EndTime;
        discount.MinBillAmount = request.MinBillAmount;
        discount.IsActive = request.IsActive;
        discount.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Discounts.Update(discount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Discount {DiscountCode} updated successfully", discountCode);

        return _mapper.Map<DiscountDto>(discount);
    }

    public async Task DeleteAsync(string discountCode, CancellationToken cancellationToken = default)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(discountCode, cancellationToken)
            ?? throw new NotFoundException("Discount", discountCode);

        _unitOfWork.Discounts.Remove(discount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Discount {DiscountCode} deleted successfully", discountCode);
    }

    public async Task<DiscountEvaluationResultDto> EvaluateAsync(EvaluateDiscountRequestDto request, CancellationToken cancellationToken = default)
    {
        var date = request.EvaluationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = new DiscountEvaluationResultDto();

        var itemLevel = new List<ApplicableDiscountDto>();
        if (!string.IsNullOrWhiteSpace(request.ItemCode))
        {
            var candidates = await _unitOfWork.Discounts.GetActiveItemDiscountsAsync(request.ItemCode.Trim(), date, cancellationToken);

            foreach (var discount in candidates)
            {
                if (!IsWithinTimeWindow(discount, request.EvaluationTime))
                {
                    continue;
                }

                // Item_Quantity discounts additionally require the purchased quantity to meet the threshold.
                if (discount.DiscountType == DiscountType.Item_Quantity
                    && (!request.Quantity.HasValue || !discount.MinQuantity.HasValue || request.Quantity.Value < discount.MinQuantity.Value))
                {
                    continue;
                }

                var baseAmount = request.ItemAmount ?? 0m;
                itemLevel.Add(ToApplicableDto(discount, baseAmount));
            }
        }

        var billLevel = new List<ApplicableDiscountDto>();
        if (request.BillAmount.HasValue)
        {
            var candidates = await _unitOfWork.Discounts.GetActiveBillDiscountsAsync(date, cancellationToken);

            foreach (var discount in candidates)
            {
                if (!IsWithinTimeWindow(discount, request.EvaluationTime))
                {
                    continue;
                }

                // Total_Bill discounts additionally require the bill total to meet the threshold.
                if (discount.DiscountType == DiscountType.Total_Bill
                    && (!discount.MinBillAmount.HasValue || request.BillAmount.Value < discount.MinBillAmount.Value))
                {
                    continue;
                }

                billLevel.Add(ToApplicableDto(discount, request.BillAmount.Value));
            }
        }

        result.ItemLevelDiscounts = itemLevel;
        result.BillLevelDiscounts = billLevel;
        result.TotalItemDiscount = itemLevel.Sum(d => d.CalculatedAmount);
        result.TotalBillDiscount = billLevel.Sum(d => d.CalculatedAmount);

        return result;
    }

    private static string? NormalizeItemCode(string? itemCode)
    {
        return string.IsNullOrWhiteSpace(itemCode) ? null : itemCode.Trim();
    }

    private static DiscountApplicableTo ResolveApplicableTo(DiscountType discountType)
    {
        return discountType switch
        {
            DiscountType.Item or DiscountType.Item_Quantity or DiscountType.Special => DiscountApplicableTo.Selected_Items,
            DiscountType.Seasonal or DiscountType.Total_Bill => DiscountApplicableTo.Entire_Bill,
            _ => DiscountApplicableTo.Entire_Bill
        };
    }

    private static bool IsWithinTimeWindow(Discount discount, TimeOnly? evaluationTime)
    {
        if (discount.StartTime is null && discount.EndTime is null)
        {
            return true;
        }

        // A time window is defined but the caller didn't provide a time to check against - treat as not applicable.
        if (evaluationTime is null)
        {
            return false;
        }

        if (discount.StartTime.HasValue && evaluationTime.Value < discount.StartTime.Value)
        {
            return false;
        }

        if (discount.EndTime.HasValue && evaluationTime.Value > discount.EndTime.Value)
        {
            return false;
        }

        return true;
    }

    private static ApplicableDiscountDto ToApplicableDto(Discount discount, decimal baseAmount)
    {
        var value = discount.DiscountValue ?? 0m;

        var calculatedAmount = discount.DiscountMethod == DiscountMethod.Percentage
            ? Math.Round(baseAmount * value / 100m, 2)
            : value;

        return new ApplicableDiscountDto
        {
            DiscountCode = discount.DiscountCode,
            DiscountName = discount.DiscountName,
            DiscountType = discount.DiscountType,
            DiscountMethod = discount.DiscountMethod,
            DiscountValue = value,
            CalculatedAmount = calculatedAmount
        };
    }
}
