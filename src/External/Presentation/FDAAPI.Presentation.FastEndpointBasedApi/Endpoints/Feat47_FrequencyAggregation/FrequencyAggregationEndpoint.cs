using FastEndpoints;
using FDAAPI.App.FeatG47_FrequencyAggregation;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat47_FrequencyAggregation.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat47_FrequencyAggregation
{
    public class FrequencyAggregationEndpoint : Endpoint<FrequencyAggregationRequestDto, FrequencyAggregationResponseDto>
    {
        private readonly IMediator _mediator;

        public FrequencyAggregationEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/analytics/frequency/aggregate");
            Policies("Admin");  // Only admins can trigger aggregation jobs
            Summary(s =>
            {
                s.Summary = "Trigger frequency aggregation job";
                s.Description = "Starts a background job to aggregate flood frequency data by administrative area and time bucket";
            });
            Tags("Analytics", "Aggregation");
        }

        public override async Task HandleAsync(FrequencyAggregationRequestDto req, CancellationToken ct)
        {
            var command = new FrequencyAggregationRequest(
                req.BucketType,
                req.StartDate,
                req.EndDate,
                req.AdministrativeAreaIds
            );

            var result = await _mediator.Send(command, ct);

            var response = new FrequencyAggregationResponseDto
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

