using FluentValidation;
using PosApi.DTOs.Product;

namespace PosApi.Validators;

public class CreateItemLogValidator : AbstractValidator<CreateItemLogDto>
{
    public CreateItemLogValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OldValue).MaximumLength(255);
        RuleFor(x => x.NewValue).MaximumLength(255);
    }
}
