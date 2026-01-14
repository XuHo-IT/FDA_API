using FastEndpoints;
using FDAAPI.App.FeatG36_AreaUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat36_AreaUpdate.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat36_AreaUpdate
{
    public class UpdateAreaEndpoint : Endpoint<UpdateAreaRequestDto, UpdateAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/areas/area/{id}");
            Policies("Authority");
            Summary(s =>
            {
                s.Summary = "Update an existing monitored area";
                s.Description = "Update details of a geographic area by its unique identifier";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(UpdateAreaRequestDto req, CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            
            if (userIdClaim == null)
            {
                await SendAsync(new UpdateAreaResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var command = new UpdateAreaRequest(
                id,
                userId,
                req.Name,
                req.Latitude,
                req.Longitude,
                req.RadiusMeters,
                req.AddressText
            );

            var result = await _mediator.Send(command, ct);

            var response = new UpdateAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            if (result.Success)
            {
                await SendAsync(response, 200, ct);
            }
            else
            {
                if (result.Message == "Area not found")
                    await SendAsync(response, 404, ct);
                else if (result.Message == "Unauthorized to update this area")
                    await SendAsync(response, 403, ct);
                else
                    await SendAsync(response, 400, ct);
            }
        }
    }
}

