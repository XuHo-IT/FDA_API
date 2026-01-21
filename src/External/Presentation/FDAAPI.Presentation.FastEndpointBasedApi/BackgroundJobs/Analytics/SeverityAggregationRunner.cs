using FDAAPI.App.Common.Models.Analytics;
using FDAAPI.App.FeatG48_SeverityAggregation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Analytics
{
    public class SeverityAggregationRunner
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeverityAggregationRunner> _logger;

        public SeverityAggregationRunner(
            IMediator mediator,
            ILogger<SeverityAggregationRunner> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task RunDailyAsync()
        {
            _logger.LogInformation("Hangfire triggered daily severity aggregation");
            await _mediator.Send(new ScheduledSeverityAggregationCommand(AggregationMode.Daily));
        }

        public async Task RunWeeklyAsync()
        {
            _logger.LogInformation("Hangfire triggered weekly severity aggregation");
            await _mediator.Send(new ScheduledSeverityAggregationCommand(AggregationMode.Weekly));
        }

        public async Task RunMonthlyAsync()
        {
            _logger.LogInformation("Hangfire triggered monthly severity aggregation");
            await _mediator.Send(new ScheduledSeverityAggregationCommand(AggregationMode.Monthly));
        }
    }
}

