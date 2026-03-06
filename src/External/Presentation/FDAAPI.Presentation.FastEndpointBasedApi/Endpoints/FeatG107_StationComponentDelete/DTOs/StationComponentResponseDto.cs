namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG107_StationComponentDelete.DTOs
{
    public class StationComponentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? Id { get; set; }
    }
}
