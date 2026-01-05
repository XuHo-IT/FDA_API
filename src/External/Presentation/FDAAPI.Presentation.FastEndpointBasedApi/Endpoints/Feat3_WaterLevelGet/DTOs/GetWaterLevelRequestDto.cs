using FastEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3_WaterLevelGet.DTOs{
    /// <summary>
    /// Data Transfer Object for Get Water Level request
    /// </summary>
    public class GetWaterLevelRequestDto
    {
        [BindFrom("waterLevelId")]
        public long WaterLevelId { get; set; }
    }
}








