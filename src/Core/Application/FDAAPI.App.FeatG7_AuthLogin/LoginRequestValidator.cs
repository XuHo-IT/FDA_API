using FluentValidation;

namespace FDAAPI.App.FeatG7_AuthLogin
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            // At least one authentication method must be provided
            RuleFor(x => x)
                .Must(x => (!string.IsNullOrEmpty(x.PhoneNumber) && !string.IsNullOrEmpty(x.OtpCode)) ||
                           (!string.IsNullOrEmpty(x.Email) && !string.IsNullOrEmpty(x.Password)))
                .WithMessage("Either Phone + OTP or Email + Password must be provided.");

            // Phone validation if provided
            When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

                RuleFor(x => x.OtpCode)
                    .NotEmpty().WithMessage("OTP code is required when using phone authentication.")
                    .Length(6).WithMessage("OTP code must be 6 digits.");
            });

            // Email validation if provided
            When(x => !string.IsNullOrEmpty(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("Invalid email format.")
                    .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required when using email authentication.");
            });
        }
    }
}

