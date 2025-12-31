using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.IServices;

namespace FDAAPI.App.FeatG15
{
    /// <summary>
    /// Response from update profile operation
    /// </summary>
    public class UpdateProfileResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UpdateProfileResponseStatusCode StatusCode { get; set; }
        public UserProfileDto? Profile { get; set; }
    }
}

