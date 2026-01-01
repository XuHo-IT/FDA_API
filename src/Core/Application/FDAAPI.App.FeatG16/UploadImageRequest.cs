using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatImages
{
    public class UploadImageRequest : IFeatureRequest<UploadImageResponse>
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string Folder { get; set; } = "products";
        public string? UserId { get; set; }
    }
}
