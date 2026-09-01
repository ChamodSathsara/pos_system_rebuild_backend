using FluentValidation;
using PosApi.DTOs.Organization;

namespace PosApi.Validators;

public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.WarehouseCode)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(50);

        RuleFor(x => x.WarehouseName)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.BranchCode).MaximumLength(50);
    }
}

public class UpdateWarehouseValidator : AbstractValidator<UpdateWarehouseDto>
{
    public UpdateWarehouseValidator()
    {
        RuleFor(x => x.WarehouseName)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.BranchCode).MaximumLength(50);
    }
}
