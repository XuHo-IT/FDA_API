using FluentValidation;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public class UpdateMapPreferencesRequestValidator : AbstractValidator<UpdateMapPreferencesRequest>
    {
        public UpdateMapPreferencesRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Settings)
                .NotNull().WithMessage("Map layer settings are required.");
        }
    }
}

