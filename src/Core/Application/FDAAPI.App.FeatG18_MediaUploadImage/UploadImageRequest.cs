using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG18_MediaUploadImage
{
    public sealed record UploadImageRequest(byte[] ImageData, string FileName, string Folder, Guid? UserId) : IFeatureRequest<UploadImageResponse>;
}






