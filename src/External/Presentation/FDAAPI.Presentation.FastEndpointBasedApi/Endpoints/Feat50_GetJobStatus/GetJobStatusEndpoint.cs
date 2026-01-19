using FastEndpoints;
using FDAAPI.App.FeatG50_GetJobStatus;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat50_GetJobStatus.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat50_GetJobStatus
{
    public class GetJobStatusEndpoint : Endpoint<GetJobStatusRequestDto, GetJobStatusResponseDto>
    {
        private readonly IMediator _mediator;

        public GetJobStatusEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/analytics/jobs/{JobRunId}");
            Policies("User");
            Summary(s => s.Summary = "Get analytics job status");
            Tags("Analytics", "Jobs");
        }

        public override async Task HandleAsync(GetJobStatusRequestDto req, CancellationToken ct)
        {
            var query = new GetJobStatusRequest(req.JobRunId);
            var result = await _mediator.Send(query, ct);

            var response = new GetJobStatusResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data != null ? new JobStatusDto
                {
                    JobRunId = result.Data.JobRunId,
                    JobType = result.Data.JobType,
                    Status = result.Data.Status,
                    StartedAt = result.Data.StartedAt,
                    FinishedAt = result.Data.FinishedAt,
                    ExecutionTimeMs = result.Data.ExecutionTimeMs,
                    RecordsProcessed = result.Data.RecordsProcessed,
                    RecordsCreated = result.Data.RecordsCreated,
                    ErrorMessage = result.Data.ErrorMessage
                } : null
            };

            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}

