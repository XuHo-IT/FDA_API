namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3_SensorReadingGet.DTOs
{
    public class GetSensorReadingResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SensorReadingDataDto? Data { get; set; }
    }
}