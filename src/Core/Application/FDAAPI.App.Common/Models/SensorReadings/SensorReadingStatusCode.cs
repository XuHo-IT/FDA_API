namespace FDAAPI.App.Common.Models.SensorReadings
{
    public enum SensorReadingStatusCode
    {
        Success = 200,
        Created = 201,
        NotFound = 404,
        ValidationError = 400,
        Unauthorized = 401,
        Forbidden = 403,
        UnknownError = 500
    }
}