using FDAAPI.App.Common.Models.Analytics;
using FDAAPI.App.FeatG49_HotspotAggregation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Analytics
{
    public class HotspotAggregationRunner
    {
        private readonly IMediator _mediator;
        private readonly ILogger<HotspotAggregationRunner> _logger;

        public HotspotAggregationRunner(
            IMediator mediator,
            ILogger<HotspotAggregationRunner> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task RunWeeklyAsync()
        {
            _logger.LogInformation("Hangfire triggered weekly hotspot aggregation");
            await _mediator.Send(new ScheduledHotspotAggregationCommand(AggregationMode.Weekly));
        }
    }
}

