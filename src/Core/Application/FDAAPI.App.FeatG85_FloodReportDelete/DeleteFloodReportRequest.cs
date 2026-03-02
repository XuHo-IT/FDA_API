using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG85_FloodReportDelete
{
    public sealed record DeleteFloodReportRequest(
        Guid Id,
        Guid? UserId,
        string UserRole
    ) : IFeatureRequest<DeleteFloodReportResponse>;
}
