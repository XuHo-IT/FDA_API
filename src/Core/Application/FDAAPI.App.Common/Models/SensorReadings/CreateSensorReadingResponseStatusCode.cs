namespace FDAAPI.App.Common.Models.SensorReadings
{
    /// <summary>
    /// Status codes specific to Create Sensor Reading feature
    /// </summary>
    public enum CreateSensorReadingResponseStatusCode
    {
        Success = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        LocationNotFound = 404,
        InternalServerError = 500
    }
}
