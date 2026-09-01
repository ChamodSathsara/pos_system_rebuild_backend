using FluentValidation;
using PosApi.DTOs.Product;

namespace PosApi.Validators;

public class CreateBrandValidator : AbstractValidator<CreateBrandDto>
{
    public CreateBrandValidator()
    {
        RuleFor(x => x.BrandName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateBrandValidator : AbstractValidator<UpdateBrandDto>
{
    public UpdateBrandValidator()
    {
        RuleFor(x => x.BrandName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
