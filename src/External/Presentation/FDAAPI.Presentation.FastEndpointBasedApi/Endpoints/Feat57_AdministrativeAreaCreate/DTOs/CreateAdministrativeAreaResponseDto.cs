using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat57_AdministrativeAreaCreate.DTOs
{
    public class CreateAdministrativeAreaResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public AdministrativeAreaDto? Data { get; set; }
    }
}

