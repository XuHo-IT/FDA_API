using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG14
{
    /// <summary>
    /// Request for getting user profile
    /// </summary>
    public class GetProfileRequest : IFeatureRequest<GetProfileResponse>
    {
        public Guid UserId { get; set; }  // From JWT claims
    }
}

