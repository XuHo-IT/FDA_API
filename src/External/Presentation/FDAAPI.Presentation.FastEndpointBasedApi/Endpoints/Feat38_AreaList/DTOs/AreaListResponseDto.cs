using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaListByUser.DTOs;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat38_AreaList.DTOs
{
    public class AreaListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public List<AreaDto> Areas { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

