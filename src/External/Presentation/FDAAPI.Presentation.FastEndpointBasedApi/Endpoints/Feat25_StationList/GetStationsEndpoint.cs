using FastEndpoints;
using FDAAPI.App.FeatG25_StationList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat25_StationList.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat25_StationList
{
    public class GetStationsEndpoint : Endpoint<GetStationsRequestDto, GetStationsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetStationsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/list-stations");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Get list of monitoring stations.";
                s.Description = "Retrieve a paginated list of stations with optional filtering.";
            });
        }

        public override async Task HandleAsync(GetStationsRequestDto req, CancellationToken ct)
        {
            var appRequest = new GetStationsRequest(
                req.SearchTerm,
                req.Status,
                req.PageNumber,
                req.PageSize);

            var result = await _mediator.Send(appRequest, ct);

            await SendAsync(new GetStationsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Stations = result.Stations,
                TotalCount = result.TotalCount
            }, result.Success ? 200 : 400, ct);
        }
    }
}

