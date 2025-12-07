using FDAAPI.App.Common.Models.StaticData;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG5.DTOs;

public class GetStaticDataResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DanangCenterDto? DanangCenter { get; set; }
    public List<SensorDto> MockSensors { get; set; } = new List<SensorDto>();
    public List<FloodZoneDto> FloodZones { get; set; } = new List<FloodZoneDto>();
}
