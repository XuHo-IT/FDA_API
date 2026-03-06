namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG105_StationComponentCreate.DTOs
{
    public class CreateStationComponentRequestDto
    {
        public string ComponentType { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Notes { get; set; }
    }
}
