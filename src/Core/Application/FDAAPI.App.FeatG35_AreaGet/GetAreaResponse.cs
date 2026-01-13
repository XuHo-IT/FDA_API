using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Areas;

namespace FDAAPI.App.FeatG35_AreaGet
{
    public class GetAreaResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusCode StatusCode { get; set; }
        public AreaDto? Area { get; set; }
    }
}
