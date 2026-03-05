using FastEndpoints;
using FDAAPI.App.FeatG53_GetHotspotRankings;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat53_GetHotspotRankings.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat53_GetHotspotRankings
{
    public class GetHotspotRankingsEndpoint : Endpoint<GetHotspotRankingsRequestDto, GetHotspotRankingsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetHotspotRankingsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/analytics/hotspots");
            Policies("User");
            Summary(s => s.Summary = "Get hotspot rankings");
            Tags("Analytics", "Hotspots");
        }

        public override async Task HandleAsync(GetHotspotRankingsRequestDto req, CancellationToken ct)
        {
            var query = new GetHotspotRankingsRequest(
                req.PeriodStart,
                req.PeriodEnd,
                req.TopN,
                req.AreaLevel
            );

            var result = await _mediator.Send(query, ct);

            var response = new GetHotspotRankingsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data != null ? new HotspotRankingsDto
                {
                    PeriodStart = result.Data.PeriodStart,
                    PeriodEnd = result.Data.PeriodEnd,
                    AreaLevel = result.Data.AreaLevel,
                    Hotspots = result.Data.Hotspots.Select(h => new HotspotDto
                    {
                        AdministrativeAreaId = h.AdministrativeAreaId,
                        AdministrativeAreaName = h.AdministrativeAreaName,
                        Score = h.Score,
                        Rank = h.Rank,
                        CalculatedAt = h.CalculatedAt
                    }).ToList()
                } : null
            };

            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}

