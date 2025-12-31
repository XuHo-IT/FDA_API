namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15.DTOs
{
    /// <summary>
    /// Data Transfer Object for UpdateProfile request
    /// </summary>
    public class UpdateProfileRequestDto
    {
        /// <summary>
        /// User's full name (max 255 characters)
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// URL to user's avatar image (must be valid HTTP/HTTPS URL)
        /// </summary>
        public string? AvatarUrl { get; set; }
    }
}

