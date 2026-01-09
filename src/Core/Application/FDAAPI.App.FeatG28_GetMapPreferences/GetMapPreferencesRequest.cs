using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public sealed record GetMapPreferencesRequest(Guid UserId) : IFeatureRequest<GetMapPreferencesResponse>;
}
