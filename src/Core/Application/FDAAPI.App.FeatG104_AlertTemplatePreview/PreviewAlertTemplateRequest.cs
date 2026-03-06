using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG104_AlertTemplatePreview
{
    public sealed record PreviewAlertTemplateRequest(
        Guid? TemplateId,
        string? TitleTemplate,
        string? BodyTemplate,
        string StationName,
        decimal WaterLevel,
        decimal Threshold,
        string Severity,
        string? Address = null,
        string? Message = null
    ) : IFeatureRequest<PreviewAlertTemplateResponse>;
}
