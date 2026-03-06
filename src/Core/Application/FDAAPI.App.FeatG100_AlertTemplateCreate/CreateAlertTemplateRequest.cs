using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG100_AlertTemplateCreate
{
    public sealed record CreateAlertTemplateRequest(
        string Name,
        string Channel,
        string? Severity,
        string TitleTemplate,
        string BodyTemplate,
        bool IsActive,
        int SortOrder,
        Guid CreatedBy
    ) : IFeatureRequest<CreateAlertTemplateResponse>;
}
