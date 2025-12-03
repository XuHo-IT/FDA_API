namespace FDAAPI.App.Feat3
{
    /// <summary>
    /// Status codes specific to Feature 3 (Get Water Level)
    /// </summary>
    public enum Feat3ResponseStatusCode
    {
        Success = 200,
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        InternalServerError = 500
    }
}
