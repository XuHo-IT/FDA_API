namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1_SensorReadingCreate.DTOs
{
    public class CreateSensorReadingRequestDto
    {
        public Guid StationId { get; set; }
        public double Value { get; set; }
        public double Distance { get; set; }
        public double SensorHeight { get; set; }
        public string Unit { get; set; } = "cm";
        public int Status { get; set; }
        public DateTime MeasuredAt { get; set; }
    }
}