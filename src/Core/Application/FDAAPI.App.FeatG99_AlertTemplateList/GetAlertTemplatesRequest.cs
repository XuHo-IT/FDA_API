using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG99_AlertTemplateList
{
    public sealed record GetAlertTemplatesRequest(
        bool? IsActive = null,
        string? Channel = null,
        string? Severity = null
    ) : IFeatureRequest<GetAlertTemplatesResponse>;
}
