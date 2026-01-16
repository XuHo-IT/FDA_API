using FluentValidation;

namespace FDAAPI.App.FeatG39_SubscribeToAlerts
{
    public class SubscribeToAlertsValidator : AbstractValidator<SubscribeToAlertsRequest>
    {
        public SubscribeToAlertsValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.MinSeverity)
                .Must(s => new[] { "info", "caution", "warning", "critical" }.Contains(s.ToLower()))
                .WithMessage("MinSeverity must be: info, caution, warning, or critical");
        }
    }
}