using FluentValidation;
using PosApi.DTOs.Sale;

namespace PosApi.Validators;

public class CreateSaleReturnItemLineValidator : AbstractValidator<CreateSaleReturnItemLineDto>
{
    public CreateSaleReturnItemLineValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}

public class CreateSaleReturnValidator : AbstractValidator<CreateSaleReturnDto>
{
    public CreateSaleReturnValidator()
    {
        RuleFor(x => x.ReturnNo).MaximumLength(50);
        RuleFor(x => x.InvoiceNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Reason).MaximumLength(255);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A sale return must contain at least one line item.");
        RuleForEach(x => x.Items).SetValidator(new CreateSaleReturnItemLineValidator());
    }
}
