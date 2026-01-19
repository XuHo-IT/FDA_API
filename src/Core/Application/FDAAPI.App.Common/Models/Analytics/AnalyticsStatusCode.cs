namespace FDAAPI.App.Common.Models.Analytics
{
    public enum AnalyticsStatusCode
    {
        Success = 200,
        Accepted = 202,  // Job started
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        InternalServerError = 500
    }
}

