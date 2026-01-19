using FluentValidation;

namespace FDAAPI.App.FeatG49_HotspotAggregation
{
    public class HotspotAggregationRequestValidator : AbstractValidator<HotspotAggregationRequest>
    {
        public HotspotAggregationRequestValidator()
        {
            RuleFor(x => x.PeriodStart)
                .NotEmpty().WithMessage("Period start date is required.");

            RuleFor(x => x.PeriodEnd)
                .NotEmpty().WithMessage("Period end date is required.")
                .GreaterThan(x => x.PeriodStart).WithMessage("Period end must be after period start.");

            RuleFor(x => x.PeriodEnd)
                .Must((request, periodEnd) => (periodEnd - request.PeriodStart).TotalDays <= 365)
                .WithMessage("Period cannot exceed 365 days.");

            RuleFor(x => x.TopN)
                .GreaterThan(0).When(x => x.TopN.HasValue)
                .WithMessage("TopN must be greater than 0 if specified.");
        }
    }
}

