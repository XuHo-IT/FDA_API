using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Map;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public class UpdateMapPreferencesHandler
        : IRequestHandler<UpdateMapPreferencesRequest, UpdateMapPreferencesResponse>
    {
        private readonly IUserPreferenceRepository _preferenceRepository;

        public UpdateMapPreferencesHandler(IUserPreferenceRepository preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<UpdateMapPreferencesResponse> Handle(
            UpdateMapPreferencesRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Validate settings
                var validationError = ValidateSettings(request.Settings);
                if (validationError != null)
                {
                    return new UpdateMapPreferencesResponse
                    {
                        Success = false,
                        Message = validationError,
                        StatusCode = UpdateMapPreferencesResponseStatusCode.ValidationFailed
                    };
                }

                // 2. Serialize settings to JSON
                var jsonValue = JsonSerializer.Serialize(request.Settings, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // 3. Check if preference exists
                var existing = await _preferenceRepository.GetByUserAndKeyAsync(
                    request.UserId,
                    "map_layers",
                    ct);

                if (existing != null)
                {
                    // UPDATE existing record
                    existing.PreferenceValue = jsonValue;
                    existing.UpdatedBy = request.UserId;
                    existing.UpdatedAt = DateTime.UtcNow;

                    var updateResult = await _preferenceRepository.UpdateAsync(existing, ct);

                    if (!updateResult)
                    {
                        return new UpdateMapPreferencesResponse
                        {
                            Success = false,
                            Message = "Failed to update preferences",
                            StatusCode = UpdateMapPreferencesResponseStatusCode.UnknownError
                        };
                    }
                }
                else
                {
                    // INSERT new record
                    var newPreference = new UserPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.UserId,
                        PreferenceKey = "map_layers",
                        PreferenceValue = jsonValue,
                        CreatedBy = request.UserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = request.UserId,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _preferenceRepository.CreateAsync(newPreference, ct);
                }

                return new UpdateMapPreferencesResponse
                {
                    Success = true,
                    Message = "Map preferences updated successfully",
                    StatusCode = UpdateMapPreferencesResponseStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new UpdateMapPreferencesResponse
                {
                    Success = false,
                    Message = $"Error updating preferences: {ex.Message}",
                    StatusCode = UpdateMapPreferencesResponseStatusCode.UnknownError
                };
            }
        }

        private string? ValidateSettings(MapLayerSettings settings)
        {
            // Validate baseMap
            if (settings.BaseMap != "standard" && settings.BaseMap != "satellite" && settings.BaseMap != "hybrid")
                return "BaseMap must be 'standard' or 'satellite'";

            // Validate opacity ranges (0-100)
            if (settings.Opacity.Flood < 0 || settings.Opacity.Flood > 100)
                return "Flood opacity must be between 0 and 100";

            if (settings.Opacity.Weather < 0 || settings.Opacity.Weather > 100)
                return "Weather opacity must be between 0 and 100";

            return null; // Valid
        }
    }
}
