namespace FDAAPI.App.Common.Models.AdministrativeAreas
{
    public enum AdministrativeAreaStatusCode
    {
        Success = 200,
        Created = 201,
        InvalidData = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,                 // For duplicate name/code
        UnknownError = 500
    }
}

