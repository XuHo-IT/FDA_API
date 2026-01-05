using FastEndpoints;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4_WaterLevelDelete.DTOs{
    /// <summary>
    /// Data Transfer Object for Delete Water Level request
    /// </summary>
    public class DeleteWaterLevelRequestDto
    {
        [BindFrom("waterLevelId")]
        public long WaterLevelId { get; set; }
    }
}








