namespace FDAAPI.App.Common.Models.Areas
{
    public enum AreaStatusCode
    {
        Success = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,                 // NEW - for duplicate name/location
        UnprocessableEntity = 422,      // NEW - for no station coverage
        TooManyRequests = 429,          // NEW - for area limit exceeded
        InternalServerError = 500
    }
}

