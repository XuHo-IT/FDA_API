using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG101_AlertTemplateUpdate
{
    public sealed record UpdateAlertTemplateRequest(
        Guid Id,
        string Name,
        string Channel,
        string? Severity,
        string TitleTemplate,
        string BodyTemplate,
        bool IsActive,
        int SortOrder,
        Guid UpdatedBy
    ) : IFeatureRequest<UpdateAlertTemplateResponse>;
}
