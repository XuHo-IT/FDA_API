using FluentValidation;

namespace FDAAPI.App.FeatG13_AuthGoogleOAuthCallback
{
    public class GoogleOAuthCallbackRequestValidator : AbstractValidator<GoogleOAuthCallbackRequest>
    {
        public GoogleOAuthCallbackRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Authorization code is required.");

            RuleFor(x => x.State)
                .NotEmpty().WithMessage("State parameter is required.");
        }
    }
}

