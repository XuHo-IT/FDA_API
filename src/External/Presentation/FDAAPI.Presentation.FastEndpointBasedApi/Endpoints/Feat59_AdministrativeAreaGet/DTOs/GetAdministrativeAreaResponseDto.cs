using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat59_AdministrativeAreaGet.DTOs
{
    public class GetAdministrativeAreaResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public AdministrativeAreaDto? AdministrativeArea { get; set; }
    }
}

