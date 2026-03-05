using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users.DTOs;
using FastEndpoints;
using MediatR;
using System.Security.Claims;
using FDAAPI.App.FeatG22_UserUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat22_UserUpdate.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users
{
    public class UpdateUserEndpoint : Endpoint<UpdateUserRequestDto, UpdateUserResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateUserEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Patch("/api/v1/admin/users/{UserId}");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Update user details (Admin only)";
                s.Description = "Updates user status, roles, or profile information.";
            });
        }

        public override async Task HandleAsync(UpdateUserRequestDto req, CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                               User.FindFirst("sub")?.Value ?? 
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new UpdateUserResponseDto 
                { 
                    Success = false, 
                    Message = "Unauthorized: Could not identify admin user" 
                }, 401, ct);
                return;
            }

            var appRequest = new UpdateUserRequest(
                adminId,
                req.UserId,
                req.FullName,
                req.PhoneNumber,
                req.Status,
                req.RoleNames);

            var result = await _mediator.Send(appRequest, ct);

            await SendAsync(new UpdateUserResponseDto
            {
                Success = result.Success,
                Message = result.Message
            }, result.Success ? 200 : 400, ct);
        }
    }
}










