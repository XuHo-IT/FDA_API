using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Areas;

namespace FDAAPI.App.FeatG36_AreaUpdate
{
    public class UpdateAreaResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusCode StatusCode { get; set; }
    }
}
