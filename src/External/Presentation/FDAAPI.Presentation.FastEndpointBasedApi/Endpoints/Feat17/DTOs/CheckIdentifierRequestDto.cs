namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat17.DTOs
{
    /// <summary>
    /// DTO for check identifier request
    /// </summary>
    public class CheckIdentifierRequestDto
    {
        /// <summary>
        /// Phone number or email address
        /// </summary>
        public string Identifier { get; set; } = string.Empty;
    }
}
