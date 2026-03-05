using FastEndpoints;
using FDAAPI.App.FeatG34_AreaStatusEvaluate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat34_AreaStatusEvaluate.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat34_AreaStatusEvaluate
{
    public class AreaStatusEvaluateEndpoint : EndpointWithoutRequest<AreaStatusEvaluateResponseDto>
    {
        private readonly IMediator _mediator;

        public AreaStatusEvaluateEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/area/areas/{id}/status");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Evaluate and return flood status for an area";
                s.Description = "Calculate flood status based on nearby station readings";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var query = new AreaStatusEvaluateRequest(id);

            var result = await _mediator.Send(query, ct);

            var response = new AreaStatusEvaluateResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data != null ? new FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat34_AreaStatusEvaluate.DTOs.AreaStatusDto
                {
                    AreaId = result.Data.AreaId,
                    Status = result.Data.Status,
                    SeverityLevel = result.Data.SeverityLevel,
                    Summary = result.Data.Summary,
                    EvaluatedAt = result.Data.EvaluatedAt,
                    ContributingStations = result.Data.ContributingStations.Select(s => new FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat34_AreaStatusEvaluate.DTOs.ContributingStationDto
                    {
                        StationId = s.StationId,
                        StationCode = s.StationCode,
                        Distance = s.Distance,
                        WaterLevel = s.WaterLevel,
                        Severity = s.Severity,
                        Weight = s.Weight
                    }).ToList()
                } : null
            };

            if (result.Success)
            {
                await SendAsync(response, 200, ct);
            }
            else
            {
                await SendAsync(response, 404, ct);
            }
        }
    }
}

