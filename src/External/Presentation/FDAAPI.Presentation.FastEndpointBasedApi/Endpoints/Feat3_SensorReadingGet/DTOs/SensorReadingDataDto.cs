namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3_SensorReadingGet.DTOs
{
    public class SensorReadingDataDto
    {
        public Guid Id { get; set; }
        public Guid StationId { get; set; }
        public double Value { get; set; }
        public double Distance { get; set; }
        public double SensorHeight { get; set; }
        public string Unit { get; set; } = "cm";
        public int Status { get; set; }
        public DateTime MeasuredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
