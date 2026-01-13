using FastEndpoints;
using FDAAPI.App.FeatG37_AreaDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat37_AreaDelete.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat37_AreaDelete
{
    public class DeleteAreaEndpoint : EndpointWithoutRequest<DeleteAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/areas/{id}");
            Policies("Authority");
            Summary(s =>
            {
                s.Summary = "Delete a monitored area";
                s.Description = "Remove a geographic area by its unique identifier";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                await SendAsync(new DeleteAreaResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var command = new DeleteAreaRequest(id, userId);

            var result = await _mediator.Send(command, ct);

            var response = new DeleteAreaResponseDto
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
                else if (result.Message == "Unauthorized to delete this area")
                    await SendAsync(response, 403, ct);
                else
                    await SendAsync(response, 400, ct);
            }
        }
    }
}

