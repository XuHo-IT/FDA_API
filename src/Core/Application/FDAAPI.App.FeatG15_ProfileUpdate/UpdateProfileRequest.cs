using FDAAPI.App.Common.Features;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG15_ProfileUpdate
{
    public sealed record UpdateProfileRequest(Guid UserId, string? FullName, IFormFile? AvatarFile, string? AvatarUrl) : IFeatureRequest<UpdateProfileResponse>;
}






