using FluentValidation;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public class GetMapPreferencesRequestValidator : AbstractValidator<GetMapPreferencesRequest>
    {
        public GetMapPreferencesRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
        }
    }
}

