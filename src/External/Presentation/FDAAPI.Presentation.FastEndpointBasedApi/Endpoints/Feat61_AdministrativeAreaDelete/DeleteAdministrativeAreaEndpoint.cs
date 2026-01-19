using FastEndpoints;
using FDAAPI.App.FeatG61_AdministrativeAreaDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat61_AdministrativeAreaDelete.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat61_AdministrativeAreaDelete
{
    public class DeleteAdministrativeAreaEndpoint : EndpointWithoutRequest<DeleteAdministrativeAreaResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteAdministrativeAreaEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/admin/administrative-areas/{id}");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Delete an administrative area (Admin only)";
                s.Description = "Remove an administrative area from the system. Cannot delete if it has child areas.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (!Guid.TryParse(idStr, out var id))
            {
                await SendAsync(new DeleteAdministrativeAreaResponseDto
                {
                    Success = false,
                    Message = "Invalid administrative area ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var command = new DeleteAdministrativeAreaRequest(id);
            var result = await _mediator.Send(command, ct);

            await SendAsync(new DeleteAdministrativeAreaResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode
            }, (int)result.StatusCode, ct);
        }
    }
}

