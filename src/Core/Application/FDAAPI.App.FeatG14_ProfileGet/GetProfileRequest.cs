using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG14_ProfileGet
{
    public sealed record GetProfileRequest(Guid UserId) : IFeatureRequest<GetProfileResponse>;

}






