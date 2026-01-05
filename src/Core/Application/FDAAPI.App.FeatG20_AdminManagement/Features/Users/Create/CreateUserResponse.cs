using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG20_AdminManagement.Common;

namespace FDAAPI.App.FeatG20_AdminManagement.Features.Users.Create
{
    public class CreateUserResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdminResponseStatusCode StatusCode { get; set; }
        public Guid? UserId { get; set; }
    }
}







