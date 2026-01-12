using FDAAPI.App.Common.Features;
using MediatR;

namespace FDAAPI.App.FeatG31_GetMapCurrentStatus
{
    /// <summary>
    /// Request to get current flood status of all stations for map rendering
    /// </summary>
    public record GetMapCurrentStatusRequest(
        decimal? MinLat = null,
        decimal? MaxLat = null,
        decimal? MinLng = null,
        decimal? MaxLng = null,
        string? Status = "active"  // Filter stations by status (active, offline, maintenance)
    ) : IFeatureRequest<GetMapCurrentStatusResponse>;
}