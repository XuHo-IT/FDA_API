using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG28_GetMapPreferences;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public sealed record UpdateMapPreferencesRequest(Guid UserId, MapLayerSettings Settings) : IFeatureRequest<UpdateMapPreferencesResponse>;
}
