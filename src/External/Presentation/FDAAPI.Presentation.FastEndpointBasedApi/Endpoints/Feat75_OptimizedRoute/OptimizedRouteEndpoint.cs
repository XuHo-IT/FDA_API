using FastEndpoints;
using FDAAPI.App.FeatG75_OptimizedRoute;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat75_OptimizedRoute.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat75_OptimizedRoute
{
    public class OptimizedRouteEndpoint : Endpoint<OptimizedRouteRequestDto, OptimizedRouteResponseDto>
    {
        private readonly IMediator _mediator;

        public OptimizedRouteEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/routing/optimized-route");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Request optimized route with advanced constraints";
                s.Description = "Calculate route with waypoints, departure time flood trend projection, and response caching.";
                s.ExampleRequest = new OptimizedRouteRequestDto
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
            Tags("Routing", "Optimization");
        }

        public override async Task HandleAsync(OptimizedRouteRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
            {
                await SendAsync(new OptimizedRouteResponseDto
                {
                    Success = false,
                    Message = "Unauthorized",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var command = new OptimizedRouteRequest(
                userId,
                req.StartLatitude,
                req.StartLongitude,
                req.EndLatitude,
                req.EndLongitude,
                req.RouteProfile,
                req.MaxAlternatives,
                req.AvoidFloodedAreas,
                req.Waypoints,
                req.DepartureTime
            );

            var result = await _mediator.Send(command, ct);

            var response = new OptimizedRouteResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            };

            var httpStatusCode = result.Success ? 200 : (int)result.StatusCode;
            await SendAsync(response, httpStatusCode, ct);
        }
    }
}
