using FDAAPI.App.Common.DTOs;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat25_StationList.DTOs
{
    public class GetStationsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public List<StationDto> Stations { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
