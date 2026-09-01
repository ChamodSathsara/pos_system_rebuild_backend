using FluentValidation;
using PosApi.DTOs.Grn;

namespace PosApi.Validators;

public class CreateGrnItemLineValidator : AbstractValidator<CreateGrnItemLineDto>
{
    public CreateGrnItemLineValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BatchNo).MaximumLength(50);
    }
}

public class CreateGrnValidator : AbstractValidator<CreateGrnDto>
{
    public CreateGrnValidator()
    {
        RuleFor(x => x.GrnNo).MaximumLength(50);
        RuleFor(x => x.PoNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WarehouseCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InvoiceNo).MaximumLength(50);
        RuleFor(x => x.Remarks).MaximumLength(255);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A GRN must contain at least one line item.");
        RuleForEach(x => x.Items).SetValidator(new CreateGrnItemLineValidator());
    }
}

public class CreateGrnReturnItemLineValidator : AbstractValidator<CreateGrnReturnItemLineDto>
{
    public CreateGrnReturnItemLineValidator()
    {
        RuleFor(x => x.GrnItemId).GreaterThan(0).WithMessage("GrnItemId is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}

public class CreateGrnReturnValidator : AbstractValidator<CreateGrnReturnDto>
{
    public CreateGrnReturnValidator()
    {
        RuleFor(x => x.GrnId).GreaterThan(0).WithMessage("GrnId is required.");
        RuleFor(x => x.Reason).MaximumLength(255);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A GRN return must contain at least one line item.");
        RuleForEach(x => x.Items).SetValidator(new CreateGrnReturnItemLineValidator());
    }
}
