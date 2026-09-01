using FluentValidation;
using PosApi.DTOs.Purchase;

namespace PosApi.Validators;

public class CreatePurchaseOrderItemLineValidator : AbstractValidator<CreatePurchaseOrderItemLineDto>
{
    public CreatePurchaseOrderItemLineValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}

public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.PoNo).MaximumLength(50);
        RuleFor(x => x.VendorId).GreaterThan(0).WithMessage("VendorId is required.");
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Remarks).MaximumLength(255);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A purchase order must contain at least one line item.");
        RuleForEach(x => x.Items).SetValidator(new CreatePurchaseOrderItemLineValidator());
    }
}

public class UpdatePurchaseOrderValidator : AbstractValidator<UpdatePurchaseOrderDto>
{
    public UpdatePurchaseOrderValidator()
    {
        RuleFor(x => x.Remarks).MaximumLength(255);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A purchase order must contain at least one line item.");
        RuleForEach(x => x.Items).SetValidator(new CreatePurchaseOrderItemLineValidator());
    }
}

public class CancelPurchaseOrderValidator : AbstractValidator<CancelPurchaseOrderDto>
{
    public CancelPurchaseOrderValidator()
    {
        RuleFor(x => x.Remarks).MaximumLength(255);
    }
}