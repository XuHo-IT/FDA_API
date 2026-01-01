using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services.IServices
{
    // Application-level abstraction for image storage providers.
    // Returns high-level types (URLs, bool) so infra-specific DTOs don't leak.
    public interface IImageStorageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "profile_images");
        Task<bool> DeleteImageAsync(string imageId);
        Task<string> GetImageUrlAsync(string imageIdOrPath, int? width = null, int? height = null);
        Task<string> GetTransformedImageUrlAsync(string imageIdOrPath, string transformation);
    }
}
