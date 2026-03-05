using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat28_GetMapPreferences.DTOs
{
    public class GetMapPreferencesResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public MapLayerSettings? Settings { get; set; }
    }
}
