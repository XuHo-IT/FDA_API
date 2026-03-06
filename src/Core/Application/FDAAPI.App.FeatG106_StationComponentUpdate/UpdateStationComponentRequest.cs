using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG106_StationComponentUpdate
{
    public class UpdateStationComponentRequest : IFeatureRequest<StationComponentResponse>
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
