using FluentValidation;
using PosApi.DTOs.Stock;

namespace PosApi.Validators;

public class CreateOpeningStockValidator
    : AbstractValidator<CreateOpeningStockDto>
{
    public CreateOpeningStockValidator()
    {
        RuleFor(x => x.ItemCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.BranchCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.WarehouseCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.BatchNo)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(
                "Opening stock quantity must be greater than zero.");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit cost cannot be negative.");

        RuleFor(x => x.ReferenceNo)
            .MaximumLength(50);

        RuleFor(x => x.Remarks)
            .MaximumLength(500);

        RuleFor(x => x.OpeningDate)
            .Must(date => date == null || date <= DateTime.UtcNow)
            .WithMessage("Opening date cannot be in the future.");
    }
}