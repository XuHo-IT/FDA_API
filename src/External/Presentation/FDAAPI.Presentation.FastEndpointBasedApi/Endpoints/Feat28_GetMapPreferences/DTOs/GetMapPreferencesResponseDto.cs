namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat28_GetMapPreferences.DTOs
{
    public class GetMapPreferencesResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public MapLayerSettingsDto? Settings { get; set; }
    }

    public class MapLayerSettingsDto
    {
        public string BaseMap { get; set; } = "standard";
        public OverlaySettingsDto Overlays { get; set; } = new();
        public OpacitySettingsDto Opacity { get; set; } = new();
    }

    public class OverlaySettingsDto
    {
        public bool Flood { get; set; }
        public bool Traffic { get; set; }
        public bool Weather { get; set; }
    }

    public class OpacitySettingsDto
    {
        public int Flood { get; set; }
        public int Weather { get; set; }
    }
}
