namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2_SensorReadingUpdate.DTOs
{
    public class UpdateSensorReadingRequestDto
    {
        public Guid Id { get; set; }
        public Guid StationId { get; set; }
        public double Value { get; set; }
        public double Distance { get; set; }
        public double SensorHeight { get; set; }
        public string? Unit { get; set; }
        public int Status { get; set; }
        public DateTime MeasuredAt { get; set; }
    }
}