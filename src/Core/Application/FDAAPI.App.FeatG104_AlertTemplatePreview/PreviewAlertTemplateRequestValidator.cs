using FluentValidation;

namespace FDAAPI.App.FeatG104_AlertTemplatePreview
{
    public class PreviewAlertTemplateRequestValidator : AbstractValidator<PreviewAlertTemplateRequest>
    {
        public PreviewAlertTemplateRequestValidator()
        {
            RuleFor(x => x.StationName)
                .NotEmpty().WithMessage("StationName is required.");

            RuleFor(x => x.WaterLevel)
                .NotEmpty().WithMessage("WaterLevel is required.");

            RuleFor(x => x.Threshold)
                .NotEmpty().WithMessage("Threshold is required.");

            RuleFor(x => x.Severity)
                .NotEmpty().WithMessage("Severity is required.");

            // Either TemplateId or both TitleTemplate and BodyTemplate must be provided
            RuleFor(x => x)
                .Must(x => x.TemplateId.HasValue ||
                    (!string.IsNullOrWhiteSpace(x.TitleTemplate) && !string.IsNullOrWhiteSpace(x.BodyTemplate)))
                .WithMessage("Either TemplateId or both TitleTemplate and BodyTemplate are required.");
        }
    }
}
