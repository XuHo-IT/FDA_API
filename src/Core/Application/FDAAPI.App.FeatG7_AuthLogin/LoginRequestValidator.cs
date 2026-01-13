using FluentValidation;

namespace FDAAPI.App.FeatG7_AuthLogin
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            // Step 1: Identifier is required (can be phone or email)
            RuleFor(x => x.Identifier)
                .NotEmpty()
                .WithMessage("Identifier (phone number or email) is required.")
                .MaximumLength(255)
                .WithMessage("Identifier must not exceed 255 characters.");

            // Step 2: At least one authentication method must be provided (OTP or Password)
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.OtpCode) || !string.IsNullOrEmpty(x.Password))
                .WithMessage("Either OTP code or Password must be provided.");

            // Step 3: OTP validation if provided
            When(x => !string.IsNullOrEmpty(x.OtpCode), () =>
            {
                RuleFor(x => x.OtpCode)
                    .Length(6).WithMessage("OTP code must be exactly 6 digits.")
                    .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits.");
            });

            // Step 4: Password validation if provided
            When(x => !string.IsNullOrEmpty(x.Password), () =>
            {
                RuleFor(x => x.Password)
                    .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
            });

            // Step 5: DeviceInfo validation (optional but limited if provided)
            When(x => !string.IsNullOrEmpty(x.DeviceInfo), () =>
            {
                RuleFor(x => x.DeviceInfo)
                    .MaximumLength(500).WithMessage("Device info must not exceed 500 characters.");
            });
        }
    }
}

