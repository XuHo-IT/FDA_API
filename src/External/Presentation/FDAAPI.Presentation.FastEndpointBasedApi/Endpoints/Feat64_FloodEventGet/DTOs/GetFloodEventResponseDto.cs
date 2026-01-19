using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat64_FloodEventGet.DTOs
{
    public class GetFloodEventResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public FloodEventDto? FloodEvent { get; set; }
    }
}

