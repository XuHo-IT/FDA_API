using FastEndpoints;
using FDAAPI.App.FeatG28_GetMapPreferences;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat28_GetMapPreferences.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat28_GetMapPreferences
{
    public class GetMapPreferencesEndpoint : EndpointWithoutRequest<GetMapPreferencesResponseDto>
    {
        private readonly IMediator _mediator;

        public GetMapPreferencesEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/preferences/map-layers");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

            Summary(s =>
            {
                s.Summary = "Get user's map layer preferences";
                s.Description = "Retrieve saved map layer settings or return defaults if not found";
                s.ResponseExamples[200] = new GetMapPreferencesResponseDto
                {
                    Success = true,
                    Message = "Map preferences retrieved successfully",
                    Settings = new MapLayerSettingsDto
                    {
                        BaseMap = "standard",
                        Overlays = new OverlaySettingsDto
                        {
                            Flood = true,
                            Traffic = false,
                            Weather = false
                        },
                        Opacity = new OpacitySettingsDto
                        {
                            Flood = 80,
                            Weather = 70
                        }
                    }
                };
            });

            Tags("Map", "Preferences");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                // Extract user ID from JWT
                var userIdClaim = User.FindFirst("sub")?.Value ??
                                  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    await SendAsync(new GetMapPreferencesResponseDto
                    {
                        Success = false,
                        Message = "Invalid user authentication"
                    }, 401, ct);
                    return;
                }

                var request = new GetMapPreferencesRequest(userId);
                var result = await _mediator.Send(request, ct);

                var response = new GetMapPreferencesResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    Settings = result.Settings != null ? new MapLayerSettingsDto
                    {
                        BaseMap = result.Settings.BaseMap,
                        Overlays = new OverlaySettingsDto
                        {
                            Flood = result.Settings.Overlays.Flood,
                            Traffic = result.Settings.Overlays.Traffic,
                            Weather = result.Settings.Overlays.Weather
                        },
                        Opacity = new OpacitySettingsDto
                        {
                            Flood = result.Settings.Opacity.Flood,
                            Weather = result.Settings.Opacity.Weather
                        }
                    } : null
                };

                var statusCode = result.StatusCode switch
                {
                    GetMapPreferencesResponseStatusCode.Success => 200,
                    GetMapPreferencesResponseStatusCode.InvalidJsonFormat => 200,
                    GetMapPreferencesResponseStatusCode.UserNotFound => 404,
                    _ => 500
                };

                await SendAsync(response, statusCode, ct);
            }
            catch (Exception ex)
            {
                await SendAsync(new GetMapPreferencesResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                }, 500, ct);
            }
        }
    }
}
