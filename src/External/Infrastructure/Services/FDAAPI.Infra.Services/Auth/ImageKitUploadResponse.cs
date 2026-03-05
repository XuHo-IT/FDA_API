using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Services.Auth
{
    public class ImageKitUploadResponse
    {
        public string FileId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public int Size { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}






