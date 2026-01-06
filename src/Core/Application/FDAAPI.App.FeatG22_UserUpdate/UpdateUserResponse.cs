using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Admin;

namespace FDAAPI.App.FeatG22_UserUpdate
{
    public class UpdateUserResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdminResponseStatusCode StatusCode { get; set; }
    }
}

