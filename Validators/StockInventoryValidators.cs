using FluentValidation;
using PosApi.DTOs.Stock;

namespace PosApi.Validators;

public class CreateStockInventoryValidator : AbstractValidator<CreateStockInventoryDto>
{
    public CreateStockInventoryValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WarehouseCode).NotEmpty().MaximumLength(50);
    }
}
