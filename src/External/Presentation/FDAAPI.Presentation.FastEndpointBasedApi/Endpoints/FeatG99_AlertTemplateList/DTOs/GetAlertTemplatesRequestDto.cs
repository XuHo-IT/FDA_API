namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG99_AlertTemplateList.DTOs
{
    public class GetAlertTemplatesRequestDto
    {
        public bool? IsActive { get; set; }
        public string? Channel { get; set; }
        public string? Severity { get; set; }
    }
}
