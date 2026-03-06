namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG99_AlertTemplateList.DTOs
{
    public class GetAlertTemplatesResponseDto
    {
        public bool Success { get; set; }
        public List<AlertTemplateResponseDto> Templates { get; set; } = new();
    }

    public class AlertTemplateResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string? Severity { get; set; }
        public string TitleTemplate { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
