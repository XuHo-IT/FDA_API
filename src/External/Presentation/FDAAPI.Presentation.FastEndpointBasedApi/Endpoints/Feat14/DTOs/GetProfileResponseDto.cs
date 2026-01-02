using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat14.DTOs
{
    public class GetProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserProfileDto? Profile { get; set; }
    }
}
