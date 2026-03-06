namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG104_AlertTemplatePreview.DTOs
{
    public class PreviewAlertTemplateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Body { get; set; }
    }
}
