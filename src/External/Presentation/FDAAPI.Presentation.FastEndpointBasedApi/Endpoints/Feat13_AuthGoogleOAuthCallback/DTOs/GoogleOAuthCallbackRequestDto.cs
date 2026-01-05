namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat13_AuthGoogleOAuthCallback.DTOs{
    public class GoogleOAuthCallbackRequestDto
    {
        /// <summary>
        /// Authorization code from Google OAuth redirect
        /// Query parameter: ?code=...
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// CSRF state token (must match cached value)
        /// Query parameter: ?state=...
        /// </summary>
        public string State { get; set; } = string.Empty;
    }
}








