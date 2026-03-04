using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG83_FloodReportList
{
    public sealed record ListFloodReportsRequest(
        string? Status,
        string? Severity,
        DateTime? From,
        DateTime? To,
        int PageNumber = 1,
        int PageSize = 10
    ) : IFeatureRequest<ListFloodReportsResponse>;
}


