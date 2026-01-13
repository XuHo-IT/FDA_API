namespace FDAAPI.App.Common.Models.Map
{
    public enum GetMapCurrentStatusResponseStatusCode
    {
        Success = 200,
        NoDataFound = 404,
        ValidationError = 400,
        UnknownError = 500
    }
}
