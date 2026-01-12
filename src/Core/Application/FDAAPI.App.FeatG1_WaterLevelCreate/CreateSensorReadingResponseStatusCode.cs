namespace FDAAPI.App.FeatG1_SensorReadingCreate
{
    /// <summary>
    /// Status codes specific to Feature 1 (Create Water Level)
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







