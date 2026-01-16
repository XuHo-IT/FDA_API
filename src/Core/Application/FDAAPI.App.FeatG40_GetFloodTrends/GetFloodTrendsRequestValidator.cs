using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace FDAAPI.App.FeatG40_GetFloodTrends
{
    public class GetFloodTrendsRequestValidator : AbstractValidator<GetFloodTrendsRequest>
    {
        private static readonly string[] ValidPeriods =
            { "last7days", "last30days", "last90days", "last365days", "custom" };
        private static readonly string[] ValidGranularities = { "daily", "weekly", "monthly" };

        public GetFloodTrendsRequestValidator()
        {
            RuleFor(x => x.StationId)
                .NotEmpty().WithMessage("StationId is required.");

            RuleFor(x => x.Period)
                .Must(p => ValidPeriods.Contains(p.ToLower()))
                .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}");

            RuleFor(x => x.Granularity)
                .Must(g => ValidGranularities.Contains(g.ToLower()))
                .WithMessage($"Granularity must be one of: {string.Join(", ", ValidGranularities)}");

            // Custom period requires StartDate and EndDate
            When(x => x.Period.ToLower() == "custom", () =>
            {
                RuleFor(x => x.StartDate)
                    .NotNull().WithMessage("StartDate is required for custom period.");

                RuleFor(x => x.EndDate)
                    .NotNull().WithMessage("EndDate is required for custom period.");

                RuleFor(x => x)
                    .Must(x => x.StartDate.HasValue && x.EndDate.HasValue &&
                              x.StartDate.Value <= x.EndDate.Value)
                    .WithMessage("StartDate must be less than or equal to EndDate.");
            });
        }
    }
}
