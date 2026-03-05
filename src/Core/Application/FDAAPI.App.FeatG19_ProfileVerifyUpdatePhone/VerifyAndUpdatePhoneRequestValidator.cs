using FluentValidation;

namespace FDAAPI.App.FeatG19_ProfileVerifyUpdatePhone
{
    public class VerifyAndUpdatePhoneRequestValidator : AbstractValidator<VerifyAndUpdatePhoneRequest>
    {
        public VerifyAndUpdatePhoneRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.NewPhoneNumber)
                .NotEmpty().WithMessage("New phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(6).WithMessage("OTP code must be 6 digits.");
        }
    }
}

