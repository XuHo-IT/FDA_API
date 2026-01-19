using FastEndpoints;
using FDAAPI.App.FeatG58_AdministrativeAreaList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat58_AdministrativeAreaList.DTOs;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat58_AdministrativeAreaList
{
    public class GetAdministrativeAreasEndpoint : Endpoint<GetAdministrativeAreasRequestDto, GetAdministrativeAreasResponseDto>
    {
        private readonly IMediator _mediator;

        public GetAdministrativeAreasEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/admin/administrative-areas");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Get list of administrative areas (Admin only)";
                s.Description = "Retrieve a paginated list of administrative areas with optional filtering by level, parent, or search term.";
            });
        }

        public override async Task HandleAsync(GetAdministrativeAreasRequestDto req, CancellationToken ct)
        {
            var appRequest = new GetAdministrativeAreasRequest(
                req.SearchTerm,
                req.Level,
                req.ParentId,
                req.PageNumber,
                req.PageSize);

            var result = await _mediator.Send(appRequest, ct);

            await SendAsync(new GetAdministrativeAreasResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                AdministrativeAreas = result.AdministrativeAreas,
                TotalCount = result.TotalCount
            }, (int)result.StatusCode, ct);
        }
    }
}

