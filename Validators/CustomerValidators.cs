using FluentValidation;
using PosApi.DTOs.Customer;

namespace PosApi.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.CustomerCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerCode));

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Mobile)
            .MaximumLength(20)
            .Matches(@"^[0-9+\-\s()]*$").WithMessage("Mobile number format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Address).MaximumLength(255);

        RuleFor(x => x.CustomerType).IsInEnum();

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative.");
    }
}
