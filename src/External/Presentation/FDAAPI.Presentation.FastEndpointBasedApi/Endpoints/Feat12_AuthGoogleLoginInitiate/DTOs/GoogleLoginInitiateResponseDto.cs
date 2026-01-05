namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat12_AuthGoogleLoginInitiate.DTOs{
    public class GoogleLoginInitiateResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AuthorizationUrl { get; set; }
        public string? State { get; set; }
    }
}



