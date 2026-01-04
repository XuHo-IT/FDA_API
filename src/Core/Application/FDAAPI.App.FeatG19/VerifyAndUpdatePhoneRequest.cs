using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG15;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG19
{
    public record VerifyAndUpdatePhoneRequest(Guid UserId,string NewPhoneNumber,string OtpCode) : IRequest<UpdateProfileResponse>;

}
