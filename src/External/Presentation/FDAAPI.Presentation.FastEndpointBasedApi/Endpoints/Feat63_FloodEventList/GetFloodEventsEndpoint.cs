using FastEndpoints;
using FDAAPI.App.FeatG63_FloodEventList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat63_FloodEventList.DTOs;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat63_FloodEventList
{
    public class GetFloodEventsEndpoint : Endpoint<GetFloodEventsRequestDto, GetFloodEventsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetFloodEventsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/admin/flood-events");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Get list of flood events (Admin only)";
                s.Description = "Retrieve a paginated list of flood events with optional filtering by administrative area, date range, or search term.";
            });
        }

        public override async Task HandleAsync(GetFloodEventsRequestDto req, CancellationToken ct)
        {
            var appRequest = new GetFloodEventsRequest(
                req.SearchTerm,
                req.AdministrativeAreaId,
                req.StartDate,
                req.EndDate,
                req.PageNumber,
                req.PageSize);

            var result = await _mediator.Send(appRequest, ct);

            await SendAsync(new GetFloodEventsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                FloodEvents = result.FloodEvents,
                TotalCount = result.TotalCount
            }, (int)result.StatusCode, ct);
        }
    }
}

