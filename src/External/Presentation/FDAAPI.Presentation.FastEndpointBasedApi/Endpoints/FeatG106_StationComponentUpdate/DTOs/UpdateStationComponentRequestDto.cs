namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG106_StationComponentUpdate.DTOs
{
    public class UpdateStationComponentRequestDto
    {
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
