using FastEndpoints;
using FDAAPI.App.Common.Models.FloodHistory;
using FDAAPI.App.FeatG39_GetFloodHistory;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat39_GetFloodHistory.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat39_GetFloodHistory
{
    public class GetFloodHistoryEndpoint : Endpoint<GetFloodHistoryRequestDto, GetFloodHistoryResponseDto>
    {
        private readonly IMediator _mediator;

        public GetFloodHistoryEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/flood-history");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get flood history data for charting";
                s.Description = "Returns timeseries flood data with configurable granularity (raw, hourly, daily) for visualization";
                s.ExampleRequest = new GetFloodHistoryRequestDto
                {
                    StationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow,
                    Granularity = "hourly",
                    Limit = 500
                };
            });
            Tags("FloodHistory", "Charts", "Timeseries");
        }

        public override async Task HandleAsync(GetFloodHistoryRequestDto req, CancellationToken ct)
        {
            var query = new GetFloodHistoryRequest(
                req.StationId,
                req.StationIds,
                req.AreaId,
                req.StartDate,
                req.EndDate,
                req.Granularity,
                req.Limit,
                req.Cursor
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

            var responseDto = new GetFloodHistoryResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data,
                Pagination = result.Pagination
            };

            await SendAsync(responseDto, statusCode, ct);
        }
    }
}
