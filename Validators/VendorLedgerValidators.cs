using FluentValidation;
using PosApi.DTOs.Vendor;

namespace PosApi.Validators;

public class CreateVendorLedgerValidator : AbstractValidator<CreateVendorLedgerDto>
{
    public CreateVendorLedgerValidator()
    {
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.GrnTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReturnTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaidCredit).GreaterThanOrEqualTo(0);
    }
}

public class UpdateVendorLedgerValidator : AbstractValidator<UpdateVendorLedgerDto>
{
    public UpdateVendorLedgerValidator()
    {
        RuleFor(x => x.GrnTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReturnTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaidCredit).GreaterThanOrEqualTo(0);
    }
}

public class RecordVendorPaymentValidator : AbstractValidator<RecordVendorPaymentDto>
{
    public RecordVendorPaymentValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        RuleFor(x => x.Remarks).MaximumLength(255);
    }
}
