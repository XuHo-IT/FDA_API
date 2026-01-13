using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Areas;

namespace FDAAPI.App.FeatG37_AreaDelete
{
    public class DeleteAreaResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusCode StatusCode { get; set; }
    }
}
