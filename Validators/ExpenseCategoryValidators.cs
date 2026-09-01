using FluentValidation;
using PosApi.DTOs.Expense;

namespace PosApi.Validators;

public class CreateExpenseCategoryValidator : AbstractValidator<CreateExpenseCategoryDto>
{
    public CreateExpenseCategoryValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class UpdateExpenseCategoryValidator : AbstractValidator<UpdateExpenseCategoryDto>
{
    public UpdateExpenseCategoryValidator()
    {
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}
