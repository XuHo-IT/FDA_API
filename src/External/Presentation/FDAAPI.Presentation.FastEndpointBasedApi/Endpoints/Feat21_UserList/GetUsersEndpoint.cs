using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users.DTOs;
using FastEndpoints;
using MediatR;
using FDAAPI.App.FeatG21_UserList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat21_UserList.DTOs;

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
            Policies("Admin"); 
            Summary(s =>
            {
                s.Summary = "Get list of users (Admin only)";
                s.Description = "Returns a paginated list of users with search and filter options.";
            });
        }

        public override async Task HandleAsync(GetUsersRequestDto req, CancellationToken ct)
        {
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










