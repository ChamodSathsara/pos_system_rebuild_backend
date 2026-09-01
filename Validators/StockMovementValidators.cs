using FluentValidation;
using PosApi.DTOs.Stock;

namespace PosApi.Validators;

public class CreateStockMovementValidator : AbstractValidator<CreateStockMovementDto>
{
    public CreateStockMovementValidator()
    {
        RuleFor(x => x.StockId).GreaterThan(0);
        RuleFor(x => x.BatchId).GreaterThan(0);
        RuleFor(x => x.MovementType).IsInEnum();
        RuleFor(x => x.ReferenceType).IsInEnum();
        RuleFor(x => x.Qty).NotEqual(0).WithMessage("Movement quantity cannot be zero.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).When(x => x.UnitCost.HasValue);
        RuleFor(x => x.ReferenceNo).MaximumLength(50);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

public class UpdateStockMovementValidator : AbstractValidator<UpdateStockMovementDto>
{
    public UpdateStockMovementValidator()
    {
        RuleFor(x => x.ReferenceNo).MaximumLength(50);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}
