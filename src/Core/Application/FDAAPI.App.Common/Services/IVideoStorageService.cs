using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services
{
    public interface IVideoStorageService
    {
        Task<(string videoUrl, string? thumbnailUrl)> UploadVideoAsync(
            Stream videoStream,
            string fileName,
            string folder = "videos",
            CancellationToken ct = default);
        
        Task<bool> DeleteVideoAsync(string videoId, CancellationToken ct = default);
    }
}

