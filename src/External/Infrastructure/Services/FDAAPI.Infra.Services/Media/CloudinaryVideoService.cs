using FDAAPI.App.Common.Services;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Services.Media
{
    public class CloudinaryVideoService : IVideoStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryVideoService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string videoUrl, string? thumbnailUrl)> UploadVideoAsync(
            Stream videoStream,
            string fileName,
            string folder = "FDAAPI",
            CancellationToken ct = default)
        {
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(fileName, videoStream),
                Folder = folder
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            return (result.SecureUrl.ToString(), null);
        }

        public async Task<bool> DeleteVideoAsync(string videoId, CancellationToken ct = default)
        {
            var destroyParams = new DeletionParams(videoId)
            {
                ResourceType = ResourceType.Video
            };
            var result = await _cloudinary.DestroyAsync(destroyParams);
            return result.Result == "ok";
        }
    }
}

