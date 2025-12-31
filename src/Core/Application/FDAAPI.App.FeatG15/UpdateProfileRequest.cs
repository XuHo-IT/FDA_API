using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG15
{
    /// <summary>
    /// Request for updating user profile
    /// </summary>
    public class UpdateProfileRequest : IFeatureRequest<UpdateProfileResponse>
    {
        public Guid UserId { get; set; }  // From JWT claims
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}

