using FluentValidation;
using PosApi.DTOs.Security;

namespace PosApi.Validators;

public class CreateUserRoleValidator : AbstractValidator<CreateUserRoleDto>
{
    public CreateUserRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50);

        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class UpdateUserRoleValidator : AbstractValidator<UpdateUserRoleDto>
{
    public UpdateUserRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50);

        RuleFor(x => x.Description).MaximumLength(255);
    }
}
