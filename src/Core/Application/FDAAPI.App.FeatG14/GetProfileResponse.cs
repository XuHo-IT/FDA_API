using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.IServices;

namespace FDAAPI.App.FeatG14
{
    /// <summary>
    /// Response from get profile operation
    /// </summary>
    public class GetProfileResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GetProfileResponseStatusCode StatusCode { get; set; }
        public UserProfileDto? Profile { get; set; }
    }
}

