using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG14
{
    public class GetProfileRequest : IFeatureRequest<GetProfileResponse>
    {
        public Guid UserId { get; set; }  // From JWT claims
    }
}
