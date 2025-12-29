namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat12.DTOs
{
    public class GoogleLoginInitiateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Google authorization URL to redirect user to
        /// </summary>
        public string AuthorizationUrl { get; set; } = string.Empty;

        /// <summary>
        /// CSRF state token (for client-side validation if needed)
        /// </summary>
        public string State { get; set; } = string.Empty;
    }
}
