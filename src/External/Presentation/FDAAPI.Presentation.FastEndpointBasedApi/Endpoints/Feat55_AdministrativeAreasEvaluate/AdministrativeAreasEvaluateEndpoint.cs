using FastEndpoints;
using FDAAPI.App.FeatG55_AdministrativeAreasEvaluate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat55_AdministrativeAreasEvaluate.DTOs;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat55_AdministrativeAreasEvaluate
{
    public class AdministrativeAreasEvaluateEndpoint : EndpointWithoutRequest<AdministrativeAreasEvaluateResponseDto>
    {
        private readonly IMediator _mediator;

        public AdministrativeAreasEvaluateEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/administrative-areas/{id}/status");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Evaluate and return flood status for an administrative area";
                s.Description = "Calculate flood status based on stations in the administrative area, includes GeoJSON and full area information";
            });
            Tags("AdministrativeArea");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var query = new AdministrativeAreasEvaluateRequest(id);

            var result = await _mediator.Send(query, ct);

            var response = new AdministrativeAreasEvaluateResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data != null ? new AdministrativeAreaStatusDto
                {
                    AdministrativeAreaId = result.Data.AdministrativeAreaId,
                    Status = result.Data.Status,
                    SeverityLevel = result.Data.SeverityLevel,
                    Summary = result.Data.Summary,
                    EvaluatedAt = result.Data.EvaluatedAt,
                    ContributingStations = result.Data.ContributingStations.Select(s => new ContributingStationDto
                    {
                        StationId = s.StationId,
                        StationCode = s.StationCode,
                        Distance = s.Distance,
                        WaterLevel = s.WaterLevel,
                        Severity = s.Severity,
                        Weight = s.Weight,
                        Ward = s.Ward != null ? new ContributingWardInfoDto
                        {
                            Id = s.Ward.Id,
                            Name = s.Ward.Name,
                            Code = s.Ward.Code
                        } : null,
                        District = s.District != null ? new ContributingDistrictInfoDto
                        {
                            Id = s.District.Id,
                            Name = s.District.Name,
                            Code = s.District.Code
                        } : null
                    }).ToList(),
                    AdministrativeArea = result.Data.AdministrativeArea != null ? new AdministrativeAreaInfoDto
                    {
                        Id = result.Data.AdministrativeArea.Id,
                        Name = result.Data.AdministrativeArea.Name,
                        Level = result.Data.AdministrativeArea.Level,
                        Code = result.Data.AdministrativeArea.Code,
                        ParentId = result.Data.AdministrativeArea.ParentId,
                        ParentName = result.Data.AdministrativeArea.ParentName
                    } : null,
                    GeoJson = result.Data.GeoJson
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

