using FluentValidation;
using PosApi.DTOs.Organization;

namespace PosApi.Validators;

public class CreateCompanyValidator : AbstractValidator<CreateCompanyDto>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.CompanyCode)
            .NotEmpty().WithMessage("Company code is required.")
            .MaximumLength(50);

        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.RegistrationNo).MaximumLength(50);
        RuleFor(x => x.TaxId).MaximumLength(50);
    }
}

public class UpdateCompanyValidator : AbstractValidator<UpdateCompanyDto>
{
    public UpdateCompanyValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.RegistrationNo).MaximumLength(50);
        RuleFor(x => x.TaxId).MaximumLength(50);
    }
}
