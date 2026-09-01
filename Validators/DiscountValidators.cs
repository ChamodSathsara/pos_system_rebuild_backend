using FluentValidation;
using PosApi.DTOs.Discount;
using PosApi.Models.Enums;

namespace PosApi.Validators;

public class CreateDiscountValidator : AbstractValidator<CreateDiscountDto>
{
    public CreateDiscountValidator()
    {
        RuleFor(x => x.DiscountCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.DiscountCode));

        RuleFor(x => x.DiscountName)
            .NotEmpty().WithMessage("Discount name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DiscountType).IsInEnum();
        RuleFor(x => x.DiscountMethod).IsInEnum();

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).WithMessage("A percentage discount value cannot exceed 100.")
            .When(x => x.DiscountMethod == DiscountMethod.Percentage);

        // Item-level types require an item.
        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage("ItemCode is required for this discount type.")
            .MaximumLength(50)
            .When(x => x.DiscountType is DiscountType.Item or DiscountType.Item_Quantity or DiscountType.Special);

        // Bill-level types must not carry an item.
        RuleFor(x => x.ItemCode)
            .Empty().WithMessage("ItemCode is not applicable to this discount type.")
            .When(x => x.DiscountType is DiscountType.Seasonal or DiscountType.Total_Bill);

        // Item_Quantity requires a minimum quantity.
        RuleFor(x => x.MinQuantity)
            .NotEmpty().WithMessage("MinQuantity is required for Item_Quantity discounts.")
            .GreaterThan(0).WithMessage("MinQuantity must be greater than zero.")
            .When(x => x.DiscountType == DiscountType.Item_Quantity);

        RuleFor(x => x.MinQuantity)
            .Null().WithMessage("MinQuantity is only applicable to Item_Quantity discounts.")
            .When(x => x.DiscountType != DiscountType.Item_Quantity);

        // Total_Bill requires a minimum bill amount.
        RuleFor(x => x.MinBillAmount)
            .NotEmpty().WithMessage("MinBillAmount is required for Total_Bill discounts.")
            .GreaterThan(0).WithMessage("MinBillAmount must be greater than zero.")
            .When(x => x.DiscountType == DiscountType.Total_Bill);

        RuleFor(x => x.MinBillAmount)
            .Null().WithMessage("MinBillAmount is only applicable to Total_Bill discounts.")
            .When(x => x.DiscountType != DiscountType.Total_Bill);

        // Seasonal and Special require an explicit date period.
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required for this discount type.")
            .When(x => x.DiscountType is DiscountType.Seasonal or DiscountType.Special);

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required for this discount type.")
            .When(x => x.DiscountType is DiscountType.Seasonal or DiscountType.Special);

        RuleFor(x => x)
            .Must(x => x.EndDate!.Value >= x.StartDate!.Value)
            .WithMessage("EndDate cannot be earlier than StartDate.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x)
            .Must(x => x.EndTime!.Value > x.StartTime!.Value)
            .WithMessage("EndTime must be later than StartTime.")
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue);
    }
}

public class UpdateDiscountValidator : AbstractValidator<UpdateDiscountDto>
{
    public UpdateDiscountValidator()
    {
        RuleFor(x => x.DiscountName)
            .NotEmpty().WithMessage("Discount name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DiscountMethod).IsInEnum();

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).WithMessage("A percentage discount value cannot exceed 100.")
            .When(x => x.DiscountMethod == DiscountMethod.Percentage);

        RuleFor(x => x.ItemCode).MaximumLength(50);

        RuleFor(x => x.MinQuantity)
            .GreaterThan(0).WithMessage("MinQuantity must be greater than zero.")
            .When(x => x.MinQuantity.HasValue);

        RuleFor(x => x.MinBillAmount)
            .GreaterThan(0).WithMessage("MinBillAmount must be greater than zero.")
            .When(x => x.MinBillAmount.HasValue);

        RuleFor(x => x)
            .Must(x => x.EndDate!.Value >= x.StartDate!.Value)
            .WithMessage("EndDate cannot be earlier than StartDate.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x)
            .Must(x => x.EndTime!.Value > x.StartTime!.Value)
            .WithMessage("EndTime must be later than StartTime.")
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue);
    }
}

public class EvaluateDiscountRequestValidator : AbstractValidator<EvaluateDiscountRequestDto>
{
    public EvaluateDiscountRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.ItemAmount)
            .GreaterThanOrEqualTo(0).WithMessage("ItemAmount cannot be negative.")
            .When(x => x.ItemAmount.HasValue);

        RuleFor(x => x.BillAmount)
            .GreaterThanOrEqualTo(0).WithMessage("BillAmount cannot be negative.")
            .When(x => x.BillAmount.HasValue);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.ItemCode) || x.BillAmount.HasValue)
            .WithMessage("Supply at least ItemCode (for item-level discounts) or BillAmount (for bill-level discounts).");
    }
}
