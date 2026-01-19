using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat62_FloodEventCreate.DTOs
{
    public class CreateFloodEventResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public FloodEventDto? Data { get; set; }
    }
}

