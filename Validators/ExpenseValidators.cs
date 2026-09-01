using FluentValidation;
using PosApi.DTOs.Expense;

namespace PosApi.Validators;

public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("A valid expense category is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Expense amount must be greater than zero.");
        RuleFor(x => x.ExpenseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.ExpenseDate.HasValue)
            .WithMessage("Expense date cannot be in the future.");
        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class UpdateExpenseValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseValidator()
    {
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("A valid expense category is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Expense amount must be greater than zero.");
        RuleFor(x => x.ExpenseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.ExpenseDate.HasValue)
            .WithMessage("Expense date cannot be in the future.");
        RuleFor(x => x.Description).MaximumLength(255);
    }
}
