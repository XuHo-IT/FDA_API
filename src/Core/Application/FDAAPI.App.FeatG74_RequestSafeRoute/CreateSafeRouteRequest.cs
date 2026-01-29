using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG74_RequestSafeRoute
{
    public sealed record CreateSafeRouteRequest(
        Guid UserId,
        decimal StartLatitude,
        decimal StartLongitude,
        decimal EndLatitude,
        decimal EndLongitude,
        string RouteProfile,
        int MaxAlternatives,
        bool AvoidFloodedAreas
    ) : IFeatureRequest<SafeRouteResponse>;

}
