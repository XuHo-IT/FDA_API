namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG108_StationComponentList.DTOs
{
    public class StationComponentListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<StationComponentDto> Components { get; set; } = new();
    }
}
