namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_UserCreate.DTOs
{
    public class CreateUserResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }
}
