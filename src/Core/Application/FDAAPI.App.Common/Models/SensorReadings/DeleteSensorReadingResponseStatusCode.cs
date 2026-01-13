namespace FDAAPI.App.Common.Models.SensorReadings
{
    public enum DeleteSensorReadingResponseStatusCode
    {
        Success = 200,
        NotFound = 404,
        ValidationError = 400,
        UnknownError = 500
    }
}
