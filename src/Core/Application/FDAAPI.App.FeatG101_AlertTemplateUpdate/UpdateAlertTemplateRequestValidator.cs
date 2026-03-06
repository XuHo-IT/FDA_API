using FluentValidation;

namespace FDAAPI.App.FeatG101_AlertTemplateUpdate
{
    public class UpdateAlertTemplateRequestValidator : AbstractValidator<UpdateAlertTemplateRequest>
    {
        private static readonly string[] ValidChannels = { "Push", "Email", "SMS", "InApp" };
        private static readonly string[] ValidSeverities = { "info", "caution", "warning", "critical" };

        public UpdateAlertTemplateRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Channel)
                .NotEmpty().WithMessage("Channel is required.")
                .Must(c => ValidChannels.Contains(c, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Channel must be one of: {string.Join(", ", ValidChannels)}");

            RuleFor(x => x.Severity)
                .Must(s => string.IsNullOrWhiteSpace(s) || ValidSeverities.Contains(s.ToLower()))
                .WithMessage($"Severity must be one of: {string.Join(", ", ValidSeverities)} (or null/empty).");

            RuleFor(x => x.TitleTemplate)
                .NotEmpty().WithMessage("TitleTemplate is required.")
                .MaximumLength(200).WithMessage("TitleTemplate cannot exceed 200 characters.");

            RuleFor(x => x.BodyTemplate)
                .NotEmpty().WithMessage("BodyTemplate is required.")
                .MaximumLength(2000).WithMessage("BodyTemplate cannot exceed 2000 characters.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("SortOrder must be >= 0.");
        }
    }
}
