using FluentValidation;
using PosApi.DTOs.Stock;

namespace PosApi.Validators;

public class CreateDamageItemValidator : AbstractValidator<CreateDamageItemDto>
{
    public CreateDamageItemValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WarehouseCode).MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Damage quantity must be greater than zero.");
        RuleFor(x => x.CostAmount).GreaterThanOrEqualTo(0).When(x => x.CostAmount.HasValue);
        RuleFor(x => x.Reason).MaximumLength(255);
        RuleFor(x => x.DamageDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.DamageDate.HasValue)
            .WithMessage("Damage date cannot be in the future.");
    }
}

public class UpdateDamageItemValidator : AbstractValidator<UpdateDamageItemDto>
{
    public UpdateDamageItemValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WarehouseCode).MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Damage quantity must be greater than zero.");
        RuleFor(x => x.CostAmount).GreaterThanOrEqualTo(0).When(x => x.CostAmount.HasValue);
        RuleFor(x => x.Reason).MaximumLength(255);
        RuleFor(x => x.DamageDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.DamageDate.HasValue)
            .WithMessage("Damage date cannot be in the future.");
        RuleFor(x => x.Status).IsInEnum();
    }
}
