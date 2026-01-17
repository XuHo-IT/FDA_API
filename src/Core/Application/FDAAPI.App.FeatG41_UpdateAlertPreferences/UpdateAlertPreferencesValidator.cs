using FluentValidation;

namespace FDAAPI.App.FeatG41_UpdateAlertPreferences
{
    public class UpdateAlertPreferencesValidator : AbstractValidator<UpdateAlertPreferencesRequest>
    {
        public UpdateAlertPreferencesValidator()
        {
            RuleFor(x => x.SubscriptionId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.MinSeverity)
                .Must(s => s == null || new[] { "info", "caution", "warning", "critical" }.Contains(s.ToLower()))
                .WithMessage("MinSeverity must be: info, caution, warning, or critical");
        }
    }
}