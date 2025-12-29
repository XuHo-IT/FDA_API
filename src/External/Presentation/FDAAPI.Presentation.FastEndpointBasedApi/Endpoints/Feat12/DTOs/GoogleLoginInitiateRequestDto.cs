namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat12.DTOs
{
    public class GoogleLoginInitiateRequestDto
    {
        /// <summary>
        /// Optional return URL after OAuth completion
        /// Will be stored in Redis state cache
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}
