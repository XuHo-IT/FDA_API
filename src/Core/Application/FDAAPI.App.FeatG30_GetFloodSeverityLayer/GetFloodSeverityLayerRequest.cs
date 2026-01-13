using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer
{
    public sealed record GetFloodSeverityLayerRequest(BoundingBox? Bounds, int? ZoomLevel) : IFeatureRequest<GetFloodSeverityLayerResponse>;
}
