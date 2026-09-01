using FluentValidation;
using PosApi.DTOs.Purchase;

namespace PosApi.Validators;

public class CreatePurchaseOrderHistoryChangeValidator : AbstractValidator<CreatePurchaseOrderHistoryChangeDto>
{
    public CreatePurchaseOrderHistoryChangeValidator()
    {
        RuleFor(x => x.HistoryId).GreaterThan(0);
        RuleFor(x => x.Field).IsInEnum();
        RuleFor(x => x.OldValue).MaximumLength(255);
        RuleFor(x => x.NewValue).MaximumLength(255);
    }
}

public class UpdatePurchaseOrderHistoryChangeValidator : AbstractValidator<UpdatePurchaseOrderHistoryChangeDto>
{
    public UpdatePurchaseOrderHistoryChangeValidator()
    {
        RuleFor(x => x.Field).IsInEnum();
        RuleFor(x => x.OldValue).MaximumLength(255);
        RuleFor(x => x.NewValue).MaximumLength(255);
    }
}