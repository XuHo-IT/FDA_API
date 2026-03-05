using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG84_FloodReportGetNearby
{
    public sealed record GetNearbyFloodReportsRequest(
        decimal Latitude,
        decimal Longitude,
        int RadiusMeters = 500,
        int Hours = 2
    ) : IFeatureRequest<GetNearbyFloodReportsResponse>;
}


