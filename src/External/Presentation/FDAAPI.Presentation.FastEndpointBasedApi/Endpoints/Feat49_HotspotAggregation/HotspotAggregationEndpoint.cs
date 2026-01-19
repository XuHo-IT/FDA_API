using FastEndpoints;
using FDAAPI.App.FeatG49_HotspotAggregation;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat49_HotspotAggregation.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat49_HotspotAggregation
{
    public class HotspotAggregationEndpoint : Endpoint<HotspotAggregationRequestDto, HotspotAggregationResponseDto>
    {
        private readonly IMediator _mediator;

        public HotspotAggregationEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/analytics/hotspots/aggregate");
            Policies("Admin");
            Summary(s => s.Summary = "Trigger hotspot aggregation job");
            Tags("Analytics", "Aggregation", "Hotspots");
        }

        public override async Task HandleAsync(HotspotAggregationRequestDto req, CancellationToken ct)
        {
            var command = new HotspotAggregationRequest(
                req.PeriodStart,
                req.PeriodEnd,
                req.TopN
            );

            var result = await _mediator.Send(command, ct);

            var response = new HotspotAggregationResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data != null ? new JobRunDto
                {
                    JobRunId = result.Data.JobRunId,
                    JobType = result.Data.JobType,
                    Status = result.Data.Status,
                    StartedAt = result.Data.StartedAt
                } : null
            };

            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}

