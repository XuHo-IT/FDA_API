using FDAAPI.App.Common.Models.Routing;

namespace FDAAPI.App.Common.Services;

public interface IGraphHopperService
{
    Task<GraphHopperRouteResponse> GetRouteAsync(
        GraphHopperRouteRequest request,
        CancellationToken ct = default);
}
