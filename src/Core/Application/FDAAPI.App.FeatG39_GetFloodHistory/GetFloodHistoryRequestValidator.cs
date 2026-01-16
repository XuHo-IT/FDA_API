using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace FDAAPI.App.FeatG39_GetFloodHistory
{
    public class GetFloodHistoryRequestValidator : AbstractValidator<GetFloodHistoryRequest>
    {
        private static readonly string[] ValidGranularities = { "raw", "hourly", "daily" };

        public GetFloodHistoryRequestValidator()
        {
            // At least one filter required
            RuleFor(x => x)
                .Must(x => x.StationId.HasValue ||
                          (x.StationIds != null && x.StationIds.Any()) ||
                          x.AreaId.HasValue)
                .WithMessage("At least one of StationId, StationIds, or AreaId is required.");

            // Validate StationId if provided
            When(x => x.StationId.HasValue, () =>
            {
                RuleFor(x => x.StationId!.Value)
                    .NotEmpty().WithMessage("StationId cannot be empty GUID.");
            });

            // Validate date range
            When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.StartDate!.Value <= x.EndDate!.Value)
                    .WithMessage("StartDate must be less than or equal to EndDate.");

                // Max 1 year for raw data
                RuleFor(x => x)
                    .Must(x => x.Granularity != "raw" ||
                              (x.EndDate!.Value - x.StartDate!.Value).TotalDays <= 365)
                    .WithMessage("Time range cannot exceed 1 year for raw granularity. Use 'hourly' or 'daily' for longer ranges.");

                // Max 5 years for aggregated data
                RuleFor(x => x)
                    .Must(x => (x.EndDate!.Value - x.StartDate!.Value).TotalDays <= 1825)
                    .WithMessage("Time range cannot exceed 5 years.");
            });

            // Validate granularity
            RuleFor(x => x.Granularity)
                .Must(g => ValidGranularities.Contains(g.ToLower()))
                .WithMessage($"Granularity must be one of: {string.Join(", ", ValidGranularities)}");

            // Validate limit
            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 10000)
                .WithMessage("Limit must be between 1 and 10000.");
        }
    }
}
