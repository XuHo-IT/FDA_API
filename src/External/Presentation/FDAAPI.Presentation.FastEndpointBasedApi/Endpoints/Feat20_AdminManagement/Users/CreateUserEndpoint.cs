using FDAAPI.App.FeatG20_AdminManagement.Features.Users.Create;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users.DTOs;
using FastEndpoints;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users
{
    public class CreateUserEndpoint : Endpoint<CreateUserRequestDto, CreateUserResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateUserEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/admin/users");
            // Policies("Admin"); // Handle manually for custom message
            Summary(s =>
            {
                s.Summary = "Create a new user (Admin only)";
                s.Description = "Creates a new user account with specified roles.";
            });
        }

        public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
        {
            if (!User.IsInRole("ADMIN") && !User.IsInRole("SUPERADMIN"))
            {
                await SendAsync(new CreateUserResponseDto 
                { 
                    Success = false, 
                    Message = "You are not allowed to access this feature." 
                }, 403, ct);
                return;
            }

            // Try multiple ways to get the user ID from claims
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                               User.FindFirst("sub")?.Value ?? 
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new CreateUserResponseDto 
                { 
                    Success = false, 
                    Message = "Unauthorized: Could not identify admin user" 
                }, 401, ct);
                return;
            }

            var appRequest = new CreateUserRequest(
                adminId,
                req.Email,
                req.Password,
                req.FullName,
                req.PhoneNumber,
                req.RoleNames);

            var result = await _mediator.Send(appRequest, ct);

            await SendAsync(new CreateUserResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                UserId = result.UserId
            }, result.Success ? 201 : 400, ct);
        }
    }
}










