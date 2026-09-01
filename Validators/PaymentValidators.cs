using FluentValidation;
using PosApi.DTOs.Payment;

namespace PosApi.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.InvoiceNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.ReferenceNo).MaximumLength(100);
    }
}

public class CancelPaymentValidator : AbstractValidator<CancelPaymentDto>
{
    public CancelPaymentValidator()
    {
        RuleFor(x => x.Remarks).MaximumLength(255);
    }
}
