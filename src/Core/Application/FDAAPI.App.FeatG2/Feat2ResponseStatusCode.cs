namespace FDAAPI.App.Feat2
{
    /// <summary>
    /// Status codes specific to Feature 2 (Update Water Level)
    /// </summary>
    public enum Feat2ResponseStatusCode
    {
        Success = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        InternalServerError = 500
    }
}
