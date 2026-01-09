using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat28_GetMapPreferences.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat29_UpdateMapPreferences.DTOs
{
    public class UpdateMapPreferencesRequestDto
    {
        public string BaseMap { get; set; } = "standard";
        public OverlaySettingsDto Overlays { get; set; } = new();
        public OpacitySettingsDto Opacity { get; set; } = new();
    }

}
