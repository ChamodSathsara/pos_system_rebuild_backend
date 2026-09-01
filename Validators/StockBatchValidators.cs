using FluentValidation;
using PosApi.DTOs.Stock;

namespace PosApi.Validators;

public class CreateStockBatchValidator : AbstractValidator<CreateStockBatchDto>
{
    public CreateStockBatchValidator()
    {
        RuleFor(x => x.StockId).GreaterThan(0);
        RuleFor(x => x.BatchNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReceivedQty).GreaterThan(0).WithMessage("Received quantity must be greater than zero.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReferenceType).IsInEnum();
        RuleFor(x => x.ReferenceNo).MaximumLength(50);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

public class UpdateStockBatchValidator : AbstractValidator<UpdateStockBatchDto>
{
    public UpdateStockBatchValidator()
    {
        RuleFor(x => x.BatchNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}
