using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG14
{
    public class GetProfileResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GetProfileResponseStatusCode StatusCode { get; set; }
        public UserProfileDto? Profile { get; set; }
    }
}
