using FluentValidation;

namespace FDAAPI.App.FeatG12_AuthGoogleLoginInitiate
{
    public class GoogleLoginInitiateRequestValidator : AbstractValidator<GoogleLoginInitiateRequest>
    {
        public GoogleLoginInitiateRequestValidator()
        {
            // ReturnUrl is optional, but if provided, validate it
            When(x => !string.IsNullOrEmpty(x.ReturnUrl), () =>
            {
                RuleFor(x => x.ReturnUrl)
                    .MaximumLength(500).WithMessage("Return URL must not exceed 500 characters.");
            });
        }
    }
}

