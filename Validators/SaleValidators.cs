using FluentValidation;
using PosApi.DTOs.Sale;

namespace PosApi.Validators;

public class CreateSaleItemLineValidator : AbstractValidator<CreateSaleItemLineDto>
{
    public CreateSaleItemLineValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue);
    }
}

public class CreateSalePaymentLineValidator : AbstractValidator<CreateSalePaymentLineDto>
{
    public CreateSalePaymentLineValidator()
    {
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.ReferenceNo).MaximumLength(100);
    }
}

public class CreateSaleValidator : AbstractValidator<CreateSaleDto>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.InvoiceNo).MaximumLength(50);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CustomerCode).MaximumLength(50);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A sale must contain at least one line item.");
        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemLineValidator());
        RuleForEach(x => x.Payments).SetValidator(new CreateSalePaymentLineValidator());
    }
}
