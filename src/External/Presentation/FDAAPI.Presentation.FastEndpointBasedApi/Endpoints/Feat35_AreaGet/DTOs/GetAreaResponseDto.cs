using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat35_AreaGet.DTOs
{
    public class GetAreaResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaDto? Data { get; set; }
    }
}

