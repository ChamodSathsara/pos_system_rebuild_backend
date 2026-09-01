using FluentValidation;
using PosApi.DTOs.Purchase;

namespace PosApi.Validators;

public class CreatePurchaseOrderItemValidator : AbstractValidator<CreatePurchaseOrderItemDto>
{
    public CreatePurchaseOrderItemValidator()
    {
        RuleFor(x => x.PoNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePurchaseOrderItemValidator : AbstractValidator<UpdatePurchaseOrderItemDto>
{
    public UpdatePurchaseOrderItemValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}