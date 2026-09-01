using FluentValidation;
using PosApi.DTOs.Vendor;

namespace PosApi.Validators;

public class CreateVendorValidator : AbstractValidator<CreateVendorDto>
{
    public CreateVendorValidator()
    {
        RuleFor(x => x.VendorCode).MaximumLength(50);

        RuleFor(x => x.VendorName)
            .NotEmpty().WithMessage("Vendor name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.ContactPerson).MaximumLength(100);
    }
}

public class UpdateVendorValidator : AbstractValidator<UpdateVendorDto>
{
    public UpdateVendorValidator()
    {
        RuleFor(x => x.VendorName)
            .NotEmpty().WithMessage("Vendor name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.ContactPerson).MaximumLength(100);
    }
}
