using FastEndpoints;
using FDAAPI.App.FeatG28_GetMapPreferences;
using FDAAPI.App.FeatG29_UpdateMapPreferences;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat28_GetMapPreferences.DTOs;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat29_UpdateMapPreferences.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat29_UpdateMapPreferences
{
    public class UpdateMapPreferencesEndpoint : Endpoint<UpdateMapPreferencesRequestDto, UpdateMapPreferencesResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateMapPreferencesEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Put("/api/v1/preferences/map-layers");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);

            Summary(s =>
            {
                s.Summary = "Update user's map layer preferences";
                s.Description = "Save map layer settings (creates if not exists, updates if exists)";
                s.ExampleRequest = new UpdateMapPreferencesRequestDto
                {
                    BaseMap = "satellite",
                    Overlays = new OverlaySettingsDto { Flood = true, Traffic = true, Weather = false },
                    Opacity = new OpacitySettingsDto { Flood = 90, Weather = 60 }
                };
            });

            Tags("Map", "Preferences");
        }

        public override async Task HandleAsync(
            UpdateMapPreferencesRequestDto req,
            CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value ??
                                  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    await SendAsync(new UpdateMapPreferencesResponseDto
                    {
                        Success = false,
                        Message = "Invalid user authentication"
                    }, 401, ct);
                    return;
                }

                var settings = new MapLayerSettings
                {
                    BaseMap = req.BaseMap,
                    Overlays = new OverlaySettings
                    {
                        Flood = req.Overlays.Flood,
                        Traffic = req.Overlays.Traffic,
                        Weather = req.Overlays.Weather
                    },
                    Opacity = new OpacitySettings
                    {
                        Flood = req.Opacity.Flood,
                        Weather = req.Opacity.Weather
                    }
                };

                var request = new UpdateMapPreferencesRequest(userId, settings);
                var result = await _mediator.Send(request, ct);

                var response = new UpdateMapPreferencesResponseDto
                {
                    Success = result.Success,
                    Message = result.Message
                };

                var statusCode = result.StatusCode switch
                {
                    UpdateMapPreferencesResponseStatusCode.Success => 200,
                    UpdateMapPreferencesResponseStatusCode.ValidationFailed => 400,
                    _ => 500
                };

                await SendAsync(response, statusCode, ct);
            }
            catch (Exception ex)
            {
                await SendAsync(new UpdateMapPreferencesResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                }, 500, ct);
            }
        }
    }
}
