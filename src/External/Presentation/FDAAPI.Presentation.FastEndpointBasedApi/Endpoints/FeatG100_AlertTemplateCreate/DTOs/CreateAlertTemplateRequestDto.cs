namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG100_AlertTemplateCreate.DTOs
{
    public class CreateAlertTemplateRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string? Severity { get; set; }
        public string TitleTemplate { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
