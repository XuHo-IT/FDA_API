using FluentValidation;

namespace FDAAPI.App.FeatG22_UserUpdate
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            When(x => !string.IsNullOrEmpty(x.FullName), () =>
            {
                RuleFor(x => x.FullName)
                    .MaximumLength(255).WithMessage("Full name must not exceed 255 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.Status), () =>
            {
                RuleFor(x => x.Status)
                    .Must(s => new[] { "active", "inactive", "banned" }.Contains(s))
                    .WithMessage("Status must be one of: active, inactive, banned.");
            });
        }
    }
}

