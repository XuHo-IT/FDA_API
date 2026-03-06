using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG102_AlertTemplateDelete
{
    public sealed record DeleteAlertTemplateRequest(Guid Id) : IFeatureRequest<DeleteAlertTemplateResponse>;
}
