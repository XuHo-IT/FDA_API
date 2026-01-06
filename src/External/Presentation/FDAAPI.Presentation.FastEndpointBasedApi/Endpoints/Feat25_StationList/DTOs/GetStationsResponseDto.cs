using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat25_StationList.DTOs
{
    public class GetStationsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<StationDto> Stations { get; set; } = Enumerable.Empty<StationDto>();
        public int TotalCount { get; set; }
    }
}

