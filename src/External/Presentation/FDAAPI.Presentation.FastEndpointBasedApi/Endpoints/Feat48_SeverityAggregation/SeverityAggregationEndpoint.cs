using FastEndpoints;
using FDAAPI.App.FeatG48_SeverityAggregation;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat48_SeverityAggregation.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat48_SeverityAggregation
{
    public class SeverityAggregationEndpoint : Endpoint<SeverityAggregationRequestDto, SeverityAggregationResponseDto>
    {
        private readonly IMediator _mediator;

        public SeverityAggregationEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/analytics/severity/aggregate");
            Policies("Admin");
            Summary(s => s.Summary = "Trigger severity aggregation job");
            Tags("Analytics", "Aggregation");
        }

        public override async Task HandleAsync(SeverityAggregationRequestDto req, CancellationToken ct)
        {
            var command = new SeverityAggregationRequest(
                req.BucketType,
                req.StartDate,
                req.EndDate,
                req.AdministrativeAreaIds
            );

            var result = await _mediator.Send(command, ct);

            var response = new SeverityAggregationResponseDto
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

