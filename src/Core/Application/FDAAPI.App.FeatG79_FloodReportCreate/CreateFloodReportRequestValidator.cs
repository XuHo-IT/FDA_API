using FluentValidation;

namespace FDAAPI.App.FeatG79_FloodReportCreate
{
    public sealed class CreateFloodReportRequestValidator : AbstractValidator<CreateFloodReportRequest>
    {
        public CreateFloodReportRequestValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90m, 90m)
                .WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180m, 180m)
                .WithMessage("Longitude must be between -180 and 180.");

            RuleFor(x => x.Severity)
                .NotEmpty()
                .Must(s => s == "low" || s == "medium" || s == "high")
                .WithMessage("Severity must be low, medium, or high.");

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}


