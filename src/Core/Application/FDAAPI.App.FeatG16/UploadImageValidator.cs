using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatImages
{
    public static class UploadImageValidator
    {
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public static bool Validate(byte[] data, string fileName, out string? error)
        {
            error = null;
            if (data == null || data.Length == 0)
            {
                error = "Empty image data";
                return false;
            }

            if (data.LongLength > MaxBytes)
            {
                error = $"Image exceeds maximum size of {MaxBytes} bytes";
                return false;
            }

            var ext = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
            if (!AllowedExtensions.Contains(ext))
            {
                error = "Unsupported image extension";
                return false;
            }

            return true;
        }
    }
}
