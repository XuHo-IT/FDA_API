using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG75_OptimizedRoute
{
    public sealed record OptimizedRouteRequest(
        Guid UserId,
        decimal StartLatitude,
        decimal StartLongitude,
        decimal EndLatitude,
        decimal EndLongitude,
        string RouteProfile,
        int MaxAlternatives,
        bool AvoidFloodedAreas,
        List<WaypointDto>? Waypoints,
        DateTime? DepartureTime
    ) : IFeatureRequest<OptimizedRouteResponse>;
}
