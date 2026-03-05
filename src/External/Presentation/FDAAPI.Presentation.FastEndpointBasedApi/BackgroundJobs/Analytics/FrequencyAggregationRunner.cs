using FDAAPI.App.Common.Models.Analytics;
using FDAAPI.App.FeatG47_FrequencyAggregation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Analytics
{
    public class FrequencyAggregationRunner
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FrequencyAggregationRunner> _logger;

        public FrequencyAggregationRunner(
            IMediator mediator,
            ILogger<FrequencyAggregationRunner> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task RunDailyAsync()
        {
            _logger.LogInformation("Hangfire triggered daily frequency aggregation");
            await _mediator.Send(new ScheduledFrequencyAggregationCommand(AggregationMode.Daily));
        }

        public async Task RunWeeklyAsync()
        {
            _logger.LogInformation("Hangfire triggered weekly frequency aggregation");
            await _mediator.Send(new ScheduledFrequencyAggregationCommand(AggregationMode.Weekly));
        }

        public async Task RunMonthlyAsync()
        {
            _logger.LogInformation("Hangfire triggered monthly frequency aggregation");
            await _mediator.Send(new ScheduledFrequencyAggregationCommand(AggregationMode.Monthly));
        }
    }
}

