using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG103_AlertTemplateGet
{
    public sealed record GetAlertTemplateByIdRequest(Guid Id) : IFeatureRequest<GetAlertTemplateByIdResponse>;
}
