namespace FDAAPI.App.Common.Models.Map
{
    public enum GetFloodSeverityLayerResponseStatusCode
    {
        Success = 0,
        NoStationsFound = 1,
        InvalidBounds = 2,
        UnknownError = 99
    }
}
