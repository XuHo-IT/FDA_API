using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Areas;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG38_AreaList
{
    public class AreaListResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusCode StatusCode { get; set; }
        public List<AreaDto> Areas { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

