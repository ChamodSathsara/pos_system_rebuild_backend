using FluentValidation;
using PosApi.DTOs.Purchase;
using PosApi.Models.Enums;

namespace PosApi.Validators;

public class CreatePurchaseOrderHistoryValidator : AbstractValidator<CreatePurchaseOrderHistoryDto>
{
    private static readonly PurchaseOrderHistoryAction[] AllowedManualActions =
    {
        PurchaseOrderHistoryAction.Approved,
        PurchaseOrderHistoryAction.Rejected
    };

    public CreatePurchaseOrderHistoryValidator()
    {
        RuleFor(x => x.PoNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Action)
            .Must(a => AllowedManualActions.Contains(a))
            .WithMessage("Only Approved or Rejected entries can be recorded manually; other lifecycle events are generated automatically.");
        RuleFor(x => x.Remarks).MaximumLength(255);
    }
}

public class UpdatePurchaseOrderHistoryValidator : AbstractValidator<UpdatePurchaseOrderHistoryDto>
{
    public UpdatePurchaseOrderHistoryValidator()
    {
        RuleFor(x => x.Remarks).MaximumLength(255);
    }
}