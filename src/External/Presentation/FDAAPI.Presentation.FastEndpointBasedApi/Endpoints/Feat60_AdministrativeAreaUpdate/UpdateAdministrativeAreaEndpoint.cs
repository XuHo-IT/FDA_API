using FastEndpoints;
using FDAAPI.App.FeatG60_AdministrativeAreaUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat60_AdministrativeAreaUpdate.DTOs;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat60_AdministrativeAreaUpdate
{
    public class UpdateAdministrativeAreaEndpoint : Endpoint<UpdateAdministrativeAreaRequestDto, UpdateAdministrativeAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateAdministrativeAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/admin/administrative-areas/{id}");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Update an existing administrative area (Admin only)";
                s.Description = "Update administrative area details by ID.";
            });
        }

        public override async Task HandleAsync(UpdateAdministrativeAreaRequestDto req, CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               User.FindFirst("sub")?.Value ??
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new UpdateAdministrativeAreaResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify admin user",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var idStr = Route<string>("id");
            if (!Guid.TryParse(idStr, out var id))
            {
                await SendAsync(new UpdateAdministrativeAreaResponseDto
                {
                    Success = false,
                    Message = "Invalid administrative area ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var command = new UpdateAdministrativeAreaRequest(
                id,
                req.Name,
                req.Level,
                req.ParentId,
                req.Code,
                req.Geometry,
                adminId
            );

            var result = await _mediator.Send(command, ct);

            await SendAsync(new UpdateAdministrativeAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode
            }, (int)result.StatusCode, ct);
        }
    }
}

