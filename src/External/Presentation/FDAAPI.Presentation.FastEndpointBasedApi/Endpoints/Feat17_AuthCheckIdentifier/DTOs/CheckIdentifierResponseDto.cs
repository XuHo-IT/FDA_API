namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat17_AuthCheckIdentifier.DTOs{
    /// <summary>
    /// DTO for check identifier response
    /// </summary>
    public class CheckIdentifierResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? IdentifierType { get; set; }
        public bool AccountExists { get; set; }
        public bool HasPassword { get; set; }
        public string? RequiredMethod { get; set; }
    }
}








