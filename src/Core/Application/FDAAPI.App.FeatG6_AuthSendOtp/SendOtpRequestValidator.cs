using FluentValidation;

namespace FDAAPI.App.FeatG6_AuthSendOtp
{
    public class SendOtpRequestValidator : AbstractValidator<SendOtpRequest>
    {
        public SendOtpRequestValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Identifier is required.")
                .MaximumLength(100).WithMessage("Identifier must not exceed 100 characters.");
        }
    }
}

