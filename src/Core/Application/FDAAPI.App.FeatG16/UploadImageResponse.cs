using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatImages
{
    public class UploadImageResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
