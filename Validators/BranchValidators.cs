using FluentValidation;
using PosApi.DTOs.Organization;

namespace PosApi.Validators;

public class CreateBranchValidator : AbstractValidator<CreateBranchDto>
{
    public CreateBranchValidator()
    {
        RuleFor(x => x.BranchCode)
            .NotEmpty().WithMessage("Branch code is required.")
            .MaximumLength(50);

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.CompanyCode).MaximumLength(50);
    }
}

public class UpdateBranchValidator : AbstractValidator<UpdateBranchDto>
{
    public UpdateBranchValidator()
    {
        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.CompanyCode).MaximumLength(50);
    }
}
