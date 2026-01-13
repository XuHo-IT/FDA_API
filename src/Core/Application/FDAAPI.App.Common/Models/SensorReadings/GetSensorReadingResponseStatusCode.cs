namespace FDAAPI.App.Common.Models.SensorReadings
{
    /// <summary>
    /// Status codes specific to Get Sensor Reading feature
    /// </summary>
    public enum GetSensorReadingResponseStatusCode
    {
        Success = 200,
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        InternalServerError = 500
    }
}
