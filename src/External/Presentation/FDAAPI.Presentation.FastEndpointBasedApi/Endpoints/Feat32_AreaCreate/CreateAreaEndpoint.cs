using FastEndpoints;
using FDAAPI.App.FeatG32_AreaCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat32_AreaCreate.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat32_AreaCreate
{
    public class CreateAreaEndpoint : Endpoint<CreateAreaRequestDto, CreateAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/areas/area");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Create a new monitored area";
                s.Description = "Create a geographic area for flood monitoring";
            });
            Tags("Area");
        }

        public override async Task HandleAsync(CreateAreaRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
            {
                await SendAsync(new CreateAreaResponseDto
                {
                    Success = false,
                    Message = "Unauthorized",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var command = new CreateAreaRequest(
                userId,
                req.Name,
                req.Latitude,
                req.Longitude,
                req.RadiusMeters,
                req.AddressText
            );

            var result = await _mediator.Send(command, ct);

            var response = new CreateAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            };

            if (result.Success)
            {
                await SendAsync(response, 201, ct);
            }
            else
            {
                await SendAsync(response, (int)result.StatusCode, ct);
            }
        }
    }
}
