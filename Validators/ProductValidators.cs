using FluentValidation;
using PosApi.DTOs.Product;

namespace PosApi.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.ItemCode).MaximumLength(50);
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);
        RuleFor(x => x.BrandId).GreaterThan(0).When(x => x.BrandId.HasValue);
        RuleFor(x => x.UnitOfMeasure).IsInEnum();
        RuleFor(x => x.ItemGroup).IsInEnum();
        RuleFor(x => x.Barcode).MaximumLength(50);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).When(x => x.CostPrice.HasValue);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0).When(x => x.SellingPrice.HasValue);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0).When(x => x.ReorderLevel.HasValue);
        RuleFor(x => x.TaxCode).MaximumLength(20);
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.CategoryId).GreaterThan(0).When(x => x.CategoryId.HasValue);
        RuleFor(x => x.BrandId).GreaterThan(0).When(x => x.BrandId.HasValue);
        RuleFor(x => x.UnitOfMeasure).IsInEnum();
        RuleFor(x => x.ItemGroup).IsInEnum();
        RuleFor(x => x.Barcode).MaximumLength(50);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).When(x => x.CostPrice.HasValue);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0).When(x => x.SellingPrice.HasValue);
        RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0).When(x => x.ReorderLevel.HasValue);
        RuleFor(x => x.TaxCode).MaximumLength(20);
    }
}
