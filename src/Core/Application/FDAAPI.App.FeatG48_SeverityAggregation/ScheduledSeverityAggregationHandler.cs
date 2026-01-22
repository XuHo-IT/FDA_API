using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG48_SeverityAggregation
{
    public class ScheduledSeverityAggregationHandler : IRequestHandler<ScheduledSeverityAggregationCommand, UnitResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ScheduledSeverityAggregationHandler> _logger;

        public ScheduledSeverityAggregationHandler(
            IMediator mediator,
            ILogger<ScheduledSeverityAggregationHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<UnitResponse> Handle(ScheduledSeverityAggregationCommand request, CancellationToken cancellationToken)
        {
            var (startUtc, endUtc, bucketType) = CalculateWindow(request.Mode);

            _logger.LogInformation("Scheduling severity aggregation. Mode: {Mode}, StartUtc: {Start}, EndUtc: {End}", request.Mode, startUtc, endUtc);

            await _mediator.Send(new SeverityAggregationRequest(
                bucketType,
                startUtc,
                endUtc,
                null), cancellationToken);

            return UnitResponse.SuccessResult();
        }

        private static (DateTime startUtc, DateTime endUtc, string bucketType) CalculateWindow(AggregationMode mode)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            return mode switch
            {
                AggregationMode.Daily => BuildDaily(nowVn, tz),
                AggregationMode.Weekly => BuildPreviousWeek(nowVn, tz),
                AggregationMode.Monthly => BuildPreviousMonth(nowVn, tz),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        private static (DateTime startUtc, DateTime endUtc, string bucketType) BuildDaily(DateTime nowVn, TimeZoneInfo tz)
        {
            var dayStartVn = nowVn.Date.AddDays(-1);
            var dayEndVn = dayStartVn.AddDays(1).AddTicks(-1);
            return (TimeZoneInfo.ConvertTimeToUtc(dayStartVn, tz), TimeZoneInfo.ConvertTimeToUtc(dayEndVn, tz), "day");
        }

        private static (DateTime startUtc, DateTime endUtc, string bucketType) BuildPreviousWeek(DateTime nowVn, TimeZoneInfo tz)
        {
            var currentWeekStart = nowVn.Date.AddDays(-(((int)nowVn.DayOfWeek + 6) % 7));
            var previousWeekStart = currentWeekStart.AddDays(-7);
            var previousWeekEnd = currentWeekStart.AddTicks(-1);
            return (TimeZoneInfo.ConvertTimeToUtc(previousWeekStart, tz), TimeZoneInfo.ConvertTimeToUtc(previousWeekEnd, tz), "week");
        }

        private static (DateTime startUtc, DateTime endUtc, string bucketType) BuildPreviousMonth(DateTime nowVn, TimeZoneInfo tz)
        {
            var currentMonthStart = new DateTime(nowVn.Year, nowVn.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart.AddTicks(-1);
            return (TimeZoneInfo.ConvertTimeToUtc(previousMonthStart, tz), TimeZoneInfo.ConvertTimeToUtc(previousMonthEnd, tz), "month");
        }
    }
}

