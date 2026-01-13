using FluentValidation;

namespace FDAAPI.App.FeatG16_AuthGoogleMobileLogin
{
    public class GoogleMobileLoginRequestValidator : AbstractValidator<GoogleMobileLoginRequest>
    {
        public GoogleMobileLoginRequestValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("ID token is required.");
        }
    }
}

