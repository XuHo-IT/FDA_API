using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG81_FloodReportGet
{
    public sealed record GetFloodReportRequest(
        Guid Id
    ) : IFeatureRequest<GetFloodReportResponse>;
}


