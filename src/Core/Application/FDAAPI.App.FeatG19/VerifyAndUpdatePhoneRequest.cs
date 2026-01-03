using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG15;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG19
{
    public class VerifyAndUpdatePhoneRequest : IFeatureRequest<UpdateProfileResponse>
    {
        public Guid UserId { get; set; }
        public string NewPhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}
