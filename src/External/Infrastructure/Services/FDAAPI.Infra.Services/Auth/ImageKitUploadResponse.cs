using System;

namespace FDAAPI.Infra.Services.Auth
{
    // Infra-only DTO for deserializing ImageKit responses. Not exposed to application layer.
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
