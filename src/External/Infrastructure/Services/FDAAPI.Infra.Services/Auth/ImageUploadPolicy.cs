using FDAAPI.App.Common.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Services.Auth
{
    public class ImageUploadPolicy : IImageUploadPolicy
    {
        private readonly long _maxBytes;
        private readonly string[] _allowedExtensions;

        public ImageUploadPolicy(IConfiguration configuration)
        {
            var maxBytesStr = configuration["ImageUpload:MaxBytes"];
            if (!long.TryParse(maxBytesStr, out _maxBytes))
            {
                _maxBytes = 5 * 1024 * 1024;
            }

            var section = configuration.GetSection("ImageUpload:AllowedExtensions");
            var children = section?.GetChildren();
            if (children != null && children.Any())
            {
                _allowedExtensions = children.Select(c => c.Value).Where(v => !string.IsNullOrEmpty(v)).ToArray()!;
            }
            else
            {
                _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            }
        }

        public bool IsAllowed(string? userId, string fileName, long sizeInBytes)
        {
            if (sizeInBytes <= 0 || sizeInBytes > _maxBytes) return false;
            var ext = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
            return Array.Exists(_allowedExtensions, e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}






