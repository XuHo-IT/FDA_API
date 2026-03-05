using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.SensorReadings;

namespace FDAAPI.App.FeatG3_SensorReadingGet
{
    public class GetSensorReadingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SensorReadingStatusCode StatusCode { get; set; }
        public SensorReadingDto? Data { get; set; }
    }
}