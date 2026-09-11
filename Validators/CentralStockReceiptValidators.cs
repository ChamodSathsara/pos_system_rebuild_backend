using FluentValidation;
using PosApi.DTOs.Stock;

namespace PosApi.Validators;

public class CreateCentralStockReceiptLineValidator : AbstractValidator<CreateCentralStockReceiptLineDto>
{
    public CreateCentralStockReceiptLineValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThan(0);
    }
}

public class CreateCentralStockReceiptValidator : AbstractValidator<CreateCentralStockReceiptDto>
{
    public CreateCentralStockReceiptValidator()
    {
        RuleFor(x => x.WarehouseCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReferenceNo).MaximumLength(100);
        RuleFor(x => x.Remarks).MaximumLength(500);
        RuleFor(x => x.ReceiptDate).Must(x => !x.HasValue || x.Value <= DateTime.UtcNow).WithMessage("Receipt date cannot be in the future.");
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.ItemCode.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == items.Count)
            .WithMessage("An item can only appear once in a central stock receipt.");
        RuleForEach(x => x.Items).SetValidator(new CreateCentralStockReceiptLineValidator());
    }
}
