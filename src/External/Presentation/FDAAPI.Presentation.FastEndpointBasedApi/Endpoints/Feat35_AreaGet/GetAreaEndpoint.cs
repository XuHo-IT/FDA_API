using FastEndpoints;
using FDAAPI.App.FeatG35_AreaGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat35_AreaGet.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat35_AreaGet
{
    public class GetAreaEndpoint : EndpointWithoutRequest<GetAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public GetAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/areas/{id}");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get a monitored area by ID";
                s.Description = "Retrieve details of a geographic area by its unique identifier";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var query = new GetAreaRequest(id);

            var result = await _mediator.Send(query, ct);

            var response = new GetAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Area
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

