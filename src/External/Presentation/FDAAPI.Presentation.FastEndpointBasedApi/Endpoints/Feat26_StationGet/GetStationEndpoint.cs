using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG26_StationGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat23_StationGet.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat23_StationGet
{
    public class GetStationEndpoint : EndpointWithoutRequest<GetStationResponseDto>
    {
        private readonly IMediator _mediator;

        public GetStationEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/stations/station/{id}");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Get monitoring station details.";
                s.Description = "Retrieve station information by its unique identifier.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (!Guid.TryParse(idStr, out var id))
            {
                await SendAsync(new GetStationResponseDto
                {
                    Success = false,
                    Message = "Invalid station ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var request = new GetStationRequest(id);
            var result = await _mediator.Send(request, ct);

            await SendAsync(new GetStationResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Station = result.Station
            }, (int)result.StatusCode, ct);
        }
    }
}
