using FluentValidation;
using PosApi.DTOs.Product;

namespace PosApi.Validators;

public class CreateTaxMasterValidator : AbstractValidator<CreateTaxMasterDto>
{
    public CreateTaxMasterValidator()
    {
        RuleFor(x => x.TaxCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TaxName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Percentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class UpdateTaxMasterValidator : AbstractValidator<UpdateTaxMasterDto>
{
    public UpdateTaxMasterValidator()
    {
        RuleFor(x => x.TaxName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Percentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}
