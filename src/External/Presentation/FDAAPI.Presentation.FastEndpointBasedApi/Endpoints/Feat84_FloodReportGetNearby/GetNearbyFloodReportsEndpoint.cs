using FastEndpoints;
using FDAAPI.App.FeatG84_FloodReportGetNearby;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat84_FloodReportGetNearby.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat84_FloodReportGetNearby
{
    public class GetNearbyFloodReportsEndpoint : Endpoint<GetNearbyFloodReportsRequestDto, GetNearbyFloodReportsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetNearbyFloodReportsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/flood-reports/nearby");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get nearby flood reports";
                s.Description = "Get flood reports within a specified radius and time window";
            });
            Description(b => b.Produces(200));
            Tags("FloodReports");
        }

        public override async Task HandleAsync(GetNearbyFloodReportsRequestDto req, CancellationToken ct)
        {
            var request = new GetNearbyFloodReportsRequest(
                req.Latitude,
                req.Longitude,
                req.RadiusMeters,
                req.Hours
            );

            var result = await _mediator.Send(request, ct);

            await SendAsync(new GetNearbyFloodReportsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Count = result.Count,
                ConsensusLevel = result.ConsensusLevel,
                ConsensusMessage = result.ConsensusMessage,
                Reports = result.Reports?.ConvertAll(r => new NearbyFloodReportItemDto
                {
                    Id = r.Id,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    Severity = r.Severity,
                    CreatedAt = r.CreatedAt,
                    DistanceMeters = r.DistanceMeters
                }) ?? new()
            }, 200, ct);
        }
    }
}
