using FluentValidation;
using PosApi.DTOs.Security;

namespace PosApi.Validators;

public class CreateSystemUserValidator : AbstractValidator<CreateSystemUserDto>
{
    public CreateSystemUserValidator()
    {
        RuleFor(x => x.UserCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.UserCode));

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.FullName).MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Mobile)
            .MaximumLength(20)
            .Matches(@"^[0-9+\-\s()]*$").WithMessage("Mobile number format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile));

        RuleFor(x => x.BranchCode).MaximumLength(50);
        RuleFor(x => x.RoleId).GreaterThan(0).When(x => x.RoleId.HasValue);
    }
}

public class UpdateSystemUserValidator : AbstractValidator<UpdateSystemUserDto>
{
    public UpdateSystemUserValidator()
    {
        RuleFor(x => x.FullName).MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Mobile)
            .MaximumLength(20)
            .Matches(@"^[0-9+\-\s()]*$").WithMessage("Mobile number format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile));

        RuleFor(x => x.BranchCode).MaximumLength(50);
        RuleFor(x => x.RoleId).GreaterThan(0).When(x => x.RoleId.HasValue);
    }
}
