namespace FDAAPI.App.Feat1
{
    /// <summary>
    /// Status codes specific to Feature 1 (Create Water Level)
    /// </summary>
    public enum Feat1ResponseStatusCode
    {
        Success = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        LocationNotFound = 404,
        InternalServerError = 500
    }
}

