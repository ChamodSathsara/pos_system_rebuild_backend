using FluentValidation;
using PosApi.DTOs.Security;

namespace PosApi.Validators;

public class CreatePermissionValidator : AbstractValidator<CreatePermissionDto>
{
    public CreatePermissionValidator()
    {
        RuleFor(x => x.PermissionName)
            .NotEmpty().WithMessage("Permission name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class UpdatePermissionValidator : AbstractValidator<UpdatePermissionDto>
{
    public UpdatePermissionValidator()
    {
        RuleFor(x => x.PermissionName)
            .NotEmpty().WithMessage("Permission name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(255);
    }
}
