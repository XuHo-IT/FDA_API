using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG20_AdminManagement.Common;

namespace FDAAPI.App.FeatG20_AdminManagement.Features.Users.List
{
    public class GetUsersResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdminResponseStatusCode StatusCode { get; set; }
        public IEnumerable<UserProfileDto> Users { get; set; } = new List<UserProfileDto>();
        public int TotalCount { get; set; }
    }
}







