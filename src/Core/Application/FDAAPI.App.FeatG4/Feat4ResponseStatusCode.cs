namespace FDAAPI.App.Feat4
{
    /// <summary>
    /// Status codes specific to Feature 4 (Delete Water Level)
    /// </summary>
    public enum Feat4ResponseStatusCode
    {
        Success = 204,
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        InternalServerError = 500
    }
}
