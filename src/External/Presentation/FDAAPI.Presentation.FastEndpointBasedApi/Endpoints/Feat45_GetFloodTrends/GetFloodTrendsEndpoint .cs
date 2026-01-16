using FastEndpoints;
using FDAAPI.App.Common.Models.FloodHistory;
using FDAAPI.App.FeatG45_GetFloodTrends;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat45_GetFloodTrends.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat45_GetFloodTrends
{
    public class GetFloodTrendsEndpoint : Endpoint<GetFloodTrendsRequestDto, GetFloodTrendsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetFloodTrendsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/flood-trends");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get flood trends over time";
                s.Description = "Returns aggregated flood trends with daily/weekly/monthly granularity and optional period comparison";
                s.ExampleRequest = new GetFloodTrendsRequestDto
                {
                    StationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Period = "last30days",
                    Granularity = "daily",
                    CompareWithPrevious = true
                };
            });
            Tags("FloodHistory", "Trends", "Analytics");
        }

        public override async Task HandleAsync(GetFloodTrendsRequestDto req, CancellationToken ct)
        {
            var query = new GetFloodTrendsRequest(
                req.StationId,
                req.Period,
                req.StartDate,
                req.EndDate,
                req.Granularity,
                req.CompareWithPrevious
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

            var responseDto = new GetFloodTrendsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data
            };

            await SendAsync(responseDto, statusCode, ct);
        }
    }
}
