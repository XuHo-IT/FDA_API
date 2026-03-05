namespace FDAAPI.App.Common.Models.Stations
{
    public enum StationStatusCode
    {
        Success = 200,                
        InvalidData = 400,           
        RateLimitExceeded = 429,      
        StationAlreadyExists = 409,  
        UnknownError = 500            
    }
}