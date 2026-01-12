using FDAAPI.App.Common.Models.SensorReadings;

namespace FDAAPI.App.FeatG2_SensorReadingUpdate
{
    public class UpdateSensorReadingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SensorReadingStatusCode StatusCode { get; set; }
        public Guid? SensorReadingId { get; set; }
    }
}