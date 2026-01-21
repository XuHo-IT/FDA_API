using FDAAPI.App.FeatG69_AdminGetAllSubscriptions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat69_AdminGetAllSubscriptions.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat69_AdminGetAllSubscriptions
{
    public class AdminGetAllSubscriptionsEndpoint : Endpoint<AdminGetAllSubscriptionsRequestDto, AdminGetAllSubscriptionsResponseDto>
    {
        private readonly IMediator _mediator;

        public AdminGetAllSubscriptionsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/admin/alerts/subscriptions");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Policies("Admin");  // ← Admin only

            Summary(s =>
            {
                s.Summary = "Admin: Get all alert subscriptions";
                s.Description = "Get all alert subscriptions with pagination and filters. Admin only.";
                s.ExampleRequest = new AdminGetAllSubscriptionsRequestDto
                {
                    Page = 1,
                    PageSize = 50
                };
            });

            Tags("Admin", "Alerts");
        }

        public override async Task HandleAsync(AdminGetAllSubscriptionsRequestDto req, CancellationToken ct)
        {
            var query = new AdminGetAllSubscriptionsRequest(
                req.Page,
                req.PageSize,
                req.UserId,
                req.StationId
            );

            var result = await _mediator.Send(query, ct);

            var response = new AdminGetAllSubscriptionsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Success ? new SubscriptionListDataDto
                {
                    Subscriptions = result.Subscriptions,
                    Pagination = result.Pagination
                } : null
            };

            await SendAsync(response, result.Success ? 200 : 500, ct);
        }
    }
}