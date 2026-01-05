using FDAAPI.App.FeatG20_AdminManagement.Features.Users.List;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users.DTOs;
using FastEndpoints;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users
{
    public class GetUsersEndpoint : Endpoint<GetUsersRequestDto, GetUsersResponseDto>
    {
        private readonly IMediator _mediator;

        public GetUsersEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/admin/users");
            // Policies("Admin"); // Handle manually for custom message
            Summary(s =>
            {
                s.Summary = "Get list of users (Admin only)";
                s.Description = "Returns a paginated list of users with search and filter options.";
            });
        }

        public override async Task HandleAsync(GetUsersRequestDto req, CancellationToken ct)
        {
            if (!User.IsInRole("ADMIN") && !User.IsInRole("SUPERADMIN"))
            {
                await SendAsync(new GetUsersResponseDto 
                { 
                    Success = false, 
                    Message = "You are not allowed to access this feature." 
                }, 403, ct);
                return;
            }

            var appRequest = new GetUsersRequest(
                req.SearchTerm,
                req.Role,
                req.Status,
                req.PageNumber,
                req.PageSize);

            var result = await _mediator.Send(appRequest, ct);

            await SendAsync(new GetUsersResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Users = result.Users,
                TotalCount = result.TotalCount
            }, result.Success ? 200 : 400, ct);
        }
    }
}










