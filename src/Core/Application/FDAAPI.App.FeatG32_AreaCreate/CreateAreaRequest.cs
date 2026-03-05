using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG32_AreaCreate
{
    public sealed record CreateAreaRequest(
        Guid UserId,
        string Name,
        decimal Latitude,
        decimal Longitude,
        int RadiusMeters,
        string AddressText
    ) : IFeatureRequest<CreateAreaResponse>;
}

