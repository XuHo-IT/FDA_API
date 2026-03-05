namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15_ProfileUpdate.DTOs{
    public class UpdateProfileRequestDto
    {
        /// <summary>
        /// User's full name (max 255 characters)
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// File upload for user's avatar image (multipart/form-data)
        /// </summary>
        public IFormFile? AvatarFile { get; set; }

        /// <summary>
        /// URL to user's avatar image (must be valid HTTP/HTTPS URL)
        /// </summary>
        public string? AvatarUrl { get; set; }
    }
}








