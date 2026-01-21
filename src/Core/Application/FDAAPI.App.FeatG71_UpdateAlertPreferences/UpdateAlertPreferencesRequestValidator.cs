using FluentValidation;

namespace FDAAPI.App.FeatG71_UpdateAlertPreferences
{
    public class UpdateAlertPreferencesRequestValidator : AbstractValidator<UpdateAlertPreferencesRequest>
    {
        public UpdateAlertPreferencesRequestValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty()
                .WithMessage("SubscriptionId is required");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required");

            RuleFor(x => x.MinSeverity)
                .Must(s => s == null || new[] { "info", "caution", "warning", "critical" }.Contains(s.ToLower()))
                .WithMessage("MinSeverity must be one of: info, caution, warning, critical");

            RuleFor(x => x.QuietHoursStart)
                .Must(BeValidTimeSpan)
                .When(x => x.QuietHoursStart.HasValue)
                .WithMessage("QuietHoursStart must be between 00:00:00 and 23:59:59");

            RuleFor(x => x.QuietHoursEnd)
                .Must(BeValidTimeSpan)
                .When(x => x.QuietHoursEnd.HasValue)
                .WithMessage("QuietHoursEnd must be between 00:00:00 and 23:59:59");
        }

        private bool BeValidTimeSpan(TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue) return true;
            return timeSpan.Value >= TimeSpan.Zero && timeSpan.Value < TimeSpan.FromDays(1);
        }
    }
}