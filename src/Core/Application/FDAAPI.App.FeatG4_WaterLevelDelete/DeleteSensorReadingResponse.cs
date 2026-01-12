using FDAAPI.App.Common.Models.SensorReadings;

namespace FDAAPI.App.FeatG4_SensorReadingDelete
{
    public class DeleteSensorReadingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SensorReadingStatusCode StatusCode { get; set; }
    }
}