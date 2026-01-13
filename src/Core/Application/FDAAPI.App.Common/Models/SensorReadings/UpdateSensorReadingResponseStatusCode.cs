namespace FDAAPI.App.Common.Models.SensorReadings
{
    public enum UpdateSensorReadingResponseStatusCode
    {
        Success = 200,
        NotFound = 404,
        ValidationError = 400,
        UnknownError = 500
    }
}
