using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG34_AreaStatusEvaluate
{
    public sealed record AreaStatusEvaluateRequest(
        Guid AreaId
    ) : IFeatureRequest<AreaStatusEvaluateResponse>;
}

