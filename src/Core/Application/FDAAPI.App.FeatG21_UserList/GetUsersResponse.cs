using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Admin;

namespace FDAAPI.App.FeatG21_UserList
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

