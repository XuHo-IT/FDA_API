using FastEndpoints;
using FDAAPI.App.FeatG74_RequestSafeRoute;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat74_RequestSafeRoute.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat74_RequestSafeRoute
{
    public class RequestSafeRouteEndpoint : Endpoint<RequestSafeRouteRequestDto, SafeRouteResponseDto>
    {
        private readonly IMediator _mediator;

        public RequestSafeRouteEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/routing/safe-route");
            Policies("User"); // Authenticated users only
            Summary(s =>
            {
                s.Summary = "Request safe route with flood avoidance";
                s.Description = "Calculate route from start to end point while avoiding active flood zones. Returns primary route and alternatives.";
                s.ExampleRequest = new RequestSafeRouteRequestDto
                {
                    StartLatitude = 10.762622m,
                    StartLongitude = 106.660172m,
                    EndLatitude = 10.823099m,
                    EndLongitude = 106.629664m,
                    RouteProfile = "car",
                    MaxAlternatives = 3,
                    AvoidFloodedAreas = true
                };
            });
            Tags("Routing", "Safety");
        }

        public override async Task HandleAsync(RequestSafeRouteRequestDto req, CancellationToken ct)
        {
            // Extract UserId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
            {
                await SendAsync(new SafeRouteResponseDto
                {
                    Success = false,
                    Message = "Unauthorized",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);

            // Create MediatR request
            var command = new CreateSafeRouteRequest(
                userId,
                req.StartLatitude,
                req.StartLongitude,
                req.EndLatitude,
                req.EndLongitude,
                req.RouteProfile,
                req.MaxAlternatives,
                req.AvoidFloodedAreas
            );

            // Send to handler
            var result = await _mediator.Send(command, ct);

            // Map to DTO
            var response = new SafeRouteResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            };

            // Send response
            var httpStatusCode = result.Success ? 200 : (int)result.StatusCode;
            await SendAsync(response, httpStatusCode, ct);
        }
    }

}
