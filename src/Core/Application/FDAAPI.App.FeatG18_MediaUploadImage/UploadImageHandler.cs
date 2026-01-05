using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG18_MediaUploadImage
{
    public class UploadImageHandler : IRequestHandler<UploadImageRequest, UploadImageResponse>
    {
        private readonly IImageStorageService _storage;
        private readonly IImageUploadPolicy? _policy;

        public UploadImageHandler(IImageStorageService storage, IImageUploadPolicy? policy = null)
        {
            _storage = storage;
            _policy = policy;
        }

        public async Task<UploadImageResponse> Handle(UploadImageRequest request, CancellationToken ct)
        {
            try
            {
                if (_policy != null && !_policy.IsAllowed(request.UserId?.ToString(), request.FileName, request.ImageData?.LongLength ?? 0))
                {
                    return new UploadImageResponse { Success = false, Message = "Upload not allowed by policy" };
                }

                using var ms = new System.IO.MemoryStream(request.ImageData ?? Array.Empty<byte>());
                var url = await _storage.UploadImageAsync(ms, request.FileName, request.Folder);
                if (string.IsNullOrWhiteSpace(url))
                    return new UploadImageResponse { Success = false, Message = "Storage failed to return a url" };

                return new UploadImageResponse { Success = true, Url = url };
            }
            catch (Exception ex)
            {
                return new UploadImageResponse { Success = false, Message = ex.Message };
            }
        }
    }
}






