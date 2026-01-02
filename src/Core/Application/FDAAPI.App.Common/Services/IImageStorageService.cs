using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services
{
    public interface IImageStorageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "profile_images");
        Task<bool> DeleteImageAsync(string imageId);
        Task<string> GetImageUrlAsync(string imageIdOrPath, int? width = null, int? height = null);
        Task<string> GetTransformedImageUrlAsync(string imageIdOrPath, string transformation);
    }
}
