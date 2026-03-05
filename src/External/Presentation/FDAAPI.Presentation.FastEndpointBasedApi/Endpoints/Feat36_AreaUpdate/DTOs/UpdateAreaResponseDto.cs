using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat36_AreaUpdate.DTOs
{
    public class UpdateAreaResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaDto? Data { get; set; }
    }
}

