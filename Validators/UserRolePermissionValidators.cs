using FluentValidation;
using PosApi.DTOs.Security;

namespace PosApi.Validators;

public class AssignPermissionValidator : AbstractValidator<AssignPermissionDto>
{
    public AssignPermissionValidator()
    {
        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("A valid PermissionId is required.");
    }
}
