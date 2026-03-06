namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG109_StationComponentGet.DTOs
{
    public class StationComponentDto
    {
        public Guid Id { get; set; }
        public Guid StationId { get; set; }
        public string? ComponentType { get; set; }
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FirmwareVersion { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? InstalledAt { get; set; }
        public DateTimeOffset? LastMaintenanceAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
