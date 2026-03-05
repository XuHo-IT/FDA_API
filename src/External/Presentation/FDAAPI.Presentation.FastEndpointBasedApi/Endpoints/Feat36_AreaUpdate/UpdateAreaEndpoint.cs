using FastEndpoints;
using FDAAPI.App.FeatG36_AreaUpdate;
using FDAAPI.Domain.RelationalDb.Entities;
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
            Put("/api/v1/areas/{id}");
            Policies("User");
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

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "USER";

            var command = new UpdateAreaRequest(
            id,
                userId,
                userRole,
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
                Message = result.Message,
                Data = result.Data
            };

            await SendAsync(response, (int)result.StatusCode, ct);
        }
    }
}

