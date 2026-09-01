using FluentValidation;
using PosApi.DTOs.Product;

namespace PosApi.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ParentCategoryId).GreaterThan(0).When(x => x.ParentCategoryId.HasValue);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ParentCategoryId).GreaterThan(0).When(x => x.ParentCategoryId.HasValue);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
