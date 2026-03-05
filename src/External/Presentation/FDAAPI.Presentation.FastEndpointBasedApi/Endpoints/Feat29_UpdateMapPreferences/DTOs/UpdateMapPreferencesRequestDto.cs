using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat29_UpdateMapPreferences.DTOs
{
    public class UpdateMapPreferencesRequestDto
    {
        public string BaseMap { get; set; } = "standard";
        public OverlaySettings Overlays { get; set; } = new();
        public OpacitySettings Opacity { get; set; } = new();
    }

}
