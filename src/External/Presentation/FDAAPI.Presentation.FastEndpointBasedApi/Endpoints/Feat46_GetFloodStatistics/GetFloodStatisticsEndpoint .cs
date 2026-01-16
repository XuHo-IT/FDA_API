using FastEndpoints;
using FDAAPI.App.Common.Models.FloodHistory;
using FDAAPI.App.FeatG46_GetFloodStatistics;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat46_GetFloodStatistics.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat46_GetFloodStatistics
{
    public class GetFloodStatisticsEndpoint : Endpoint<GetFloodStatisticsRequestDto, GetFloodStatisticsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetFloodStatisticsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/flood-statistics");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get flood statistics summary";
                s.Description = "Returns statistical summary with severity breakdown, data quality metrics, and period comparison";
                s.ExampleRequest = new GetFloodStatisticsRequestDto
                {
                    StationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Period = "last30days",
                    IncludeBreakdown = true,
                    IncludeComparison = true
                };
            });
            Tags("FloodHistory", "Statistics", "Analytics");
        }

        public override async Task HandleAsync(GetFloodStatisticsRequestDto req, CancellationToken ct)
        {
            var query = new GetFloodStatisticsRequest(
                req.StationId,
                req.StationIds,
                req.AreaId,
                req.Period,
                req.IncludeBreakdown,
                req.IncludeComparison
            );

            var result = await _mediator.Send(query, ct);

            var statusCode = result.StatusCode switch
            {
                FloodHistoryStatusCode.Success => 200,
                FloodHistoryStatusCode.BadRequest => 400,
                FloodHistoryStatusCode.Unauthorized => 401,
                FloodHistoryStatusCode.Forbidden => 403,
                FloodHistoryStatusCode.NotFound => 404,
                FloodHistoryStatusCode.TooManyRequests => 429,
                _ => 500
            };

            var responseDto = new GetFloodStatisticsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data
            };

            await SendAsync(responseDto, statusCode, ct);
        }
    }
}
