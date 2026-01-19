using FastEndpoints;
using FDAAPI.App.FeatG59_AdministrativeAreaGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat59_AdministrativeAreaGet.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat59_AdministrativeAreaGet
{
    public class GetAdministrativeAreaEndpoint : EndpointWithoutRequest<GetAdministrativeAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public GetAdministrativeAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/admin/administrative-areas/{id}");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Get administrative area details (Admin only)";
                s.Description = "Retrieve administrative area information by its unique identifier.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (!Guid.TryParse(idStr, out var id))
            {
                await SendAsync(new GetAdministrativeAreaResponseDto
                {
                    Success = false,
                    Message = "Invalid administrative area ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var request = new GetAdministrativeAreaRequest(id);
            var result = await _mediator.Send(request, ct);

            await SendAsync(new GetAdministrativeAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                AdministrativeArea = result.AdministrativeArea
            }, (int)result.StatusCode, ct);
        }
    }
}

