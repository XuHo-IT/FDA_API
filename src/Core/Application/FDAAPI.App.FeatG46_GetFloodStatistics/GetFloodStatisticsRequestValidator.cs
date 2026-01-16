using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace FDAAPI.App.FeatG46_GetFloodStatistics
{
    public class GetFloodStatisticsRequestValidator : AbstractValidator<GetFloodStatisticsRequest>
    {
        private static readonly string[] ValidPeriods =
            { "last7days", "last30days", "last90days", "last365days" };

        public GetFloodStatisticsRequestValidator()
        {
            // At least one filter required
            RuleFor(x => x)
                .Must(x => x.StationId.HasValue ||
                          (x.StationIds != null && x.StationIds.Any()) ||
                          x.AreaId.HasValue)
                .WithMessage("At least one of StationId, StationIds, or AreaId is required.");

            RuleFor(x => x.Period)
                .Must(p => ValidPeriods.Contains(p.ToLower()))
                .WithMessage($"Period must be one of: {string.Join(", ", ValidPeriods)}");
        }
    }
}
