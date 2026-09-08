using FluentValidation;
using PosApi.DTOs.Organization;

namespace PosApi.Validators;

public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.WarehouseCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.WarehouseCode));

        RuleFor(x => x.WarehouseName)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.BranchCode).NotEmpty().WithMessage("Branch is required for a branch warehouse.").When(x => !x.IsCentralWarehouse);
        RuleFor(x => x.BranchCode).Empty().WithMessage("A central warehouse cannot be assigned to a branch.").When(x => x.IsCentralWarehouse);
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
        RuleFor(x => x.BranchCode).NotEmpty().WithMessage("Branch is required for a branch warehouse.").When(x => !x.IsCentralWarehouse);
        RuleFor(x => x.BranchCode).Empty().WithMessage("A central warehouse cannot be assigned to a branch.").When(x => x.IsCentralWarehouse);
    }
}
