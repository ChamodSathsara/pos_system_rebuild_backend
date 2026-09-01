using FluentValidation;
using PosApi.DTOs.Cash;
using PosApi.Models.Enums;

namespace PosApi.Validators;

public class OpenCashierShiftValidator : AbstractValidator<OpenCashierShiftDto>
{
    public OpenCashierShiftValidator()
    {
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OpeningCash).GreaterThanOrEqualTo(0).WithMessage("Opening cash cannot be negative.");
    }
}

public class RecalculateCashierShiftValidator : AbstractValidator<RecalculateCashierShiftDto>
{
    public RecalculateCashierShiftValidator()
    {
        RuleFor(x => x.ActualCash).GreaterThanOrEqualTo(0).WithMessage("Actual cash cannot be negative.");
    }
}

public class CloseCashierShiftValidator : AbstractValidator<CloseCashierShiftDto>
{
    public CloseCashierShiftValidator()
    {
        RuleFor(x => x.ActualCash).GreaterThanOrEqualTo(0).WithMessage("Actual cash cannot be negative.");
        RuleFor(x => x.ReasonType).IsInEnum().When(x => x.ReasonType.HasValue);
        RuleFor(x => x.ReasonDescription).MaximumLength(500);

        // Whether a reason is mandatory at all depends on whether Actual Cash differs from the
        // computed Expected Cash, which is only known once the service loads the shift - that
        // part of the rule is enforced in CashierShiftService, not here. This rule only enforces
        // the DTO-local constraint: if "Other" is chosen, a custom description must be given.
        RuleFor(x => x.ReasonDescription)
            .NotEmpty()
            .WithMessage("A custom reason description is required when reason type is 'Other'.")
            .When(x => x.ReasonType == ShiftDifferenceReasonType.Other);
    }
}
