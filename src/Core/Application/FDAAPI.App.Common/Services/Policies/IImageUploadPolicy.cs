namespace FDAAPI.App.Common.Services.Policies
{
    // Policy to decide if an upload is permitted for a given actor or context.
    public interface IImageUploadPolicy
    {
        bool IsAllowed(string? userId, string fileName, long sizeInBytes);
    }
}
