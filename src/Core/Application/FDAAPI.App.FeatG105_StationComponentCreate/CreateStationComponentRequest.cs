using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG105_StationComponentCreate
{
    public class CreateStationComponentRequest : IFeatureRequest<StationComponentResponse>
    {
        public Guid StationId { get; set; }
        public string ComponentType { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Notes { get; set; }
    }
}
