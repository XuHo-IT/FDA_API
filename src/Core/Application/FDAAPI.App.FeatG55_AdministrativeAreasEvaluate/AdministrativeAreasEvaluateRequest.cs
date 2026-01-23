using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG55_AdministrativeAreasEvaluate
{
    public sealed record AdministrativeAreasEvaluateRequest(
        Guid AdministrativeAreaId
    ) : IFeatureRequest<AdministrativeAreasEvaluateResponse>;
}

