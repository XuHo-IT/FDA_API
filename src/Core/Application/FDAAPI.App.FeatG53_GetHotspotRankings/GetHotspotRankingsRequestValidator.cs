using FluentValidation;

namespace FDAAPI.App.FeatG53_GetHotspotRankings
{
    public class GetHotspotRankingsRequestValidator : AbstractValidator<GetHotspotRankingsRequest>
    {
        public GetHotspotRankingsRequestValidator()
        {
            RuleFor(x => x.PeriodEnd)
                .GreaterThan(x => x.PeriodStart).WithMessage("Period end must be after period start.")
                .When(x => x.PeriodStart.HasValue && x.PeriodEnd.HasValue);

            RuleFor(x => x.TopN)
                .GreaterThan(0).When(x => x.TopN.HasValue)
                .WithMessage("TopN must be greater than 0 if specified.");

            RuleFor(x => x.AreaLevel)
                .Must(BeValidAreaLevel).WithMessage("Area level must be 'ward' or 'district'.")
                .When(x => !string.IsNullOrEmpty(x.AreaLevel));
        }

        private bool BeValidAreaLevel(string? areaLevel)
        {
            return areaLevel?.ToLower() is "ward" or "district";
        }
    }
}

