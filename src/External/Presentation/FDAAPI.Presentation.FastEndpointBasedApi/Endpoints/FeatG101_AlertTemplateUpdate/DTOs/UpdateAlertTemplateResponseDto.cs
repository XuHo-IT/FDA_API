namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG101_AlertTemplateUpdate.DTOs
{
    public class UpdateAlertTemplateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? Id { get; set; }
    }
}
