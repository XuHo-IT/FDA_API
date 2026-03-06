namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG104_AlertTemplatePreview.DTOs
{
    public class PreviewAlertTemplateRequestDto
    {
        public Guid? TemplateId { get; set; }
        public string? TitleTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string StationName { get; set; } = string.Empty;
        public decimal WaterLevel { get; set; }
        public decimal Threshold { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Message { get; set; }
    }
}
