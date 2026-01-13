using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Map;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public class GetMapPreferencesHandler : IRequestHandler<GetMapPreferencesRequest, GetMapPreferencesResponse>
    {
        private readonly IUserPreferenceRepository _preferenceRepository;

        public GetMapPreferencesHandler(IUserPreferenceRepository preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<GetMapPreferencesResponse> Handle(GetMapPreferencesRequest request,CancellationToken ct)
        {
            try
            {
                // 1. Get preference from database
                var preference = await _preferenceRepository.GetByUserAndKeyAsync(request.UserId,"map_layers",ct);

                // 2. If found, deserialize and return
                if (preference != null)
                {
                    try
                    {
                        var settings = JsonSerializer.Deserialize<MapLayerSettings>(
                            preference.PreferenceValue,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return new GetMapPreferencesResponse
                        {
                            Success = true,
                            Message = "Map preferences retrieved successfully",
                            StatusCode = GetMapPreferencesResponseStatusCode.Success,
                            Settings = settings ?? GetDefaultSettings()
                        };
                    }
                    catch (JsonException)
                    {
                        // Invalid JSON in database - return defaults
                        return new GetMapPreferencesResponse
                        {
                            Success = true,
                            Message = "Invalid preferences found, returning defaults",
                            StatusCode = GetMapPreferencesResponseStatusCode.InvalidJsonFormat,
                            Settings = GetDefaultSettings()
                        };
                    }
                }

                // 3. If not found, return default settings
                return new GetMapPreferencesResponse
                {
                    Success = true,
                    Message = "No preferences found, returning defaults",
                    StatusCode = GetMapPreferencesResponseStatusCode.Success,
                    Settings = GetDefaultSettings()
                };
            }
            catch (Exception ex)
            {
                return new GetMapPreferencesResponse
                {
                    Success = false,
                    Message = $"Error retrieving preferences: {ex.Message}",
                    StatusCode = GetMapPreferencesResponseStatusCode.UnknownError,
                    Settings = GetDefaultSettings()
                };
            }
        }

        private MapLayerSettings GetDefaultSettings()
        {
            return new MapLayerSettings
            {
                BaseMap = "standard",
                Overlays = new OverlaySettings
                {
                    Flood = true,    // ON by default (app focus is flood)
                    Traffic = false,
                    Weather = false
                },
                Opacity = new OpacitySettings
                {
                    Flood = 80,
                    Weather = 70
                }
            };
        }
    }
}
