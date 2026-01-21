using FDAAPI.App.FeatG70_AdminGetAlertStats;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat70_AdminGetAlertStats
{
    public class AdminGetAlertStatsEndpoint : Endpoint<AdminGetAlertStatsRequestDto, AdminGetAlertStatsResponseDto>
    {
        private readonly IMediator _mediator;

        public AdminGetAlertStatsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/admin/alerts/stats");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Policies("Admin");  // ← Admin only

            Summary(s =>
            {
                s.Summary = "Admin: Get alert statistics";
                s.Description = "Get comprehensive alert and notification statistics for dashboard. Admin only.";
                s.ExampleRequest = new AdminGetAlertStatsRequestDto
                {
                    FromDate = DateTime.UtcNow.AddHours(-24),
                    ToDate = DateTime.UtcNow
                };
                s.Responses[200] = "Statistics retrieved successfully";
                s.Responses[401] = "Unauthorized";
                s.Responses[403] = "Forbidden (not admin)";
            });

            Tags("Admin", "Alerts", "Statistics");
        }

        public override async Task HandleAsync(AdminGetAlertStatsRequestDto req, CancellationToken ct)
        {
            var query = new AdminGetAlertStatsRequest(
                req.FromDate,
                req.ToDate
            );

            var result = await _mediator.Send(query, ct);

            var response = new AdminGetAlertStatsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data
            };

            await SendAsync(response, result.Success ? 200 : 500, ct);
        }
    }
}