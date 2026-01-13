using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public sealed record UpdateMapPreferencesRequest(Guid UserId, MapLayerSettings Settings) : IFeatureRequest<UpdateMapPreferencesResponse>;
}
