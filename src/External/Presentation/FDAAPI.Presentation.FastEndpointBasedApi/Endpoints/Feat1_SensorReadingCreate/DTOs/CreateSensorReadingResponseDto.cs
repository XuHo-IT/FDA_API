namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1_SensorReadingCreate.DTOs
{
    public class CreateSensorReadingResponseDto
    {
        public Guid? Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}