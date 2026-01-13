using FluentValidation;

namespace FDAAPI.App.FeatG15_ProfileUpdate
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            When(x => !string.IsNullOrEmpty(x.FullName), () =>
            {
                RuleFor(x => x.FullName)
                    .MaximumLength(255).WithMessage("Full name must not exceed 255 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.AvatarUrl), () =>
            {
                RuleFor(x => x.AvatarUrl)
                    .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters.");
            });
        }
    }
}

