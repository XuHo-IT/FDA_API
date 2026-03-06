namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG106_StationComponentUpdate.DTOs
{
    public class StationComponentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? Id { get; set; }
        public StationComponentDto? Component { get; set; }
    }
}
