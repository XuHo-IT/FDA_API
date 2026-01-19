using FDAAPI.App.Common.DTOs;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat58_AdministrativeAreaList.DTOs
{
    public class GetAdministrativeAreasResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public List<AdministrativeAreaDto> AdministrativeAreas { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

