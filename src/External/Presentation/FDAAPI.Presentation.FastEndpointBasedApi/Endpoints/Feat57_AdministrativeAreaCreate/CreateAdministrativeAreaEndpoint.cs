using FastEndpoints;
using FDAAPI.App.FeatG57_AdministrativeAreaCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat57_AdministrativeAreaCreate.DTOs;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat57_AdministrativeAreaCreate
{
    public class CreateAdministrativeAreaEndpoint : Endpoint<CreateAdministrativeAreaRequestDto, CreateAdministrativeAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateAdministrativeAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/admin/administrative-areas");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Create a new administrative area (Admin only)";
                s.Description = "Creates a new administrative area (city, district, or ward) with hierarchical structure.";
            });
        }

        public override async Task HandleAsync(CreateAdministrativeAreaRequestDto req, CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               User.FindFirst("sub")?.Value ??
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new CreateAdministrativeAreaResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify admin user"
                }, 401, ct);
                return;
            }

            var command = new CreateAdministrativeAreaRequest(
                req.Name,
                req.Level,
                req.ParentId,
                req.Code,
                req.Geometry,
                adminId
            );

            var result = await _mediator.Send(command, ct);

            await SendAsync(new CreateAdministrativeAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            }, (int)result.StatusCode, ct);
        }
    }
}

