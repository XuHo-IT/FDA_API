using FDAAPI.App.Common.Features;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG15
{
    public class UpdateProfileRequest : IFeatureRequest<UpdateProfileResponse>
    {
        public Guid UserId { get; set; }  // From JWT claims
        public string? FullName { get; set; }

        public IFormFile? AvatarFile { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
