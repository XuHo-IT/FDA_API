namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat82_FloodReportUpdate.DTOs
{
    public class UpdateFloodReportRequestDto
    {
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Severity { get; set; }

        // For adding media via file upload or URL
        public List<IFormFile>? MediaFilesToAdd { get; set; }
        // For deleting media, pass media IDs (Guid)
        public List<Guid>? MediaToDelete { get; set; }
            // Removed MediaToUpdate
    }

    public class MediaAddDto
    {
        public string MediaType { get; set; } = "photo";
        public string MediaUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
    }

}
