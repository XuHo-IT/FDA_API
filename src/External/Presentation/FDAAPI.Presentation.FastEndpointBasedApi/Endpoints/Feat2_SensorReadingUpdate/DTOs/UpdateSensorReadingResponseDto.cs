namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2_SensorReadingUpdate.DTOs
{
    public class UpdateSensorReadingResponseDto
    {
        public Guid? Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}