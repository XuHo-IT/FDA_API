namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG100_AlertTemplateCreate.DTOs
{
    public class CreateAlertTemplateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? Id { get; set; }
    }
}
