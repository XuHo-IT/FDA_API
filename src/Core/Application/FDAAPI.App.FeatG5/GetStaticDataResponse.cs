using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.StaticData;

namespace FDAAPI.App.FeatG5;

public class GetStaticDataResponse : IFeatureResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DanangCenterDto? DanangCenter { get; set; }
    public List<SensorDto> MockSensors { get; set; } = new List<SensorDto>();
    public List<FloodZoneDto> FloodZones { get; set; } = new List<FloodZoneDto>();
}
