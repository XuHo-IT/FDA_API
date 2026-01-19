namespace FDAAPI.App.Common.Models.FloodEvents
{
    public enum FloodEventStatusCode
    {
        Success = 200,
        Created = 201,
        InvalidData = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,                 // For duplicate or overlapping events
        UnknownError = 500
    }
}

