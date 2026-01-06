using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat23_StationGet.DTOs
{
    public class GetStationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Station? Station { get; set; }
    }
}

