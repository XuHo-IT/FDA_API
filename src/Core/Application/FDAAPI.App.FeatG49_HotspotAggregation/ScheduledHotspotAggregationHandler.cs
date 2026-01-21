using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG49_HotspotAggregation
{
    public class ScheduledHotspotAggregationHandler : IFeatureHandler<ScheduledHotspotAggregationCommand, UnitResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ScheduledHotspotAggregationHandler> _logger;

        public ScheduledHotspotAggregationHandler(
            IMediator mediator,
            ILogger<ScheduledHotspotAggregationHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<UnitResponse> ExecuteAsync(ScheduledHotspotAggregationCommand request, CancellationToken cancellationToken)
        {
            if (request.Mode != AggregationMode.Weekly)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Mode), "Hotspot scheduled aggregation supports weekly mode only.");
            }

            var (periodStartUtc, periodEndUtc) = CalculatePreviousWeek();

            _logger.LogInformation("Scheduling hotspot aggregation for previous week. StartUtc: {Start}, EndUtc: {End}", periodStartUtc, periodEndUtc);

            await _mediator.Send(new HotspotAggregationRequest(
                periodStartUtc,
                periodEndUtc,
                50), cancellationToken);

            return UnitResponse.SuccessResult();
        }

        private static (DateTime startUtc, DateTime endUtc) CalculatePreviousWeek()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            var currentWeekStart = nowVn.Date.AddDays(-(((int)nowVn.DayOfWeek + 6) % 7));
            var previousWeekStart = currentWeekStart.AddDays(-7);
            var previousWeekEnd = currentWeekStart.AddTicks(-1);

            return (TimeZoneInfo.ConvertTimeToUtc(previousWeekStart, tz), TimeZoneInfo.ConvertTimeToUtc(previousWeekEnd, tz));
        }
    }
}

