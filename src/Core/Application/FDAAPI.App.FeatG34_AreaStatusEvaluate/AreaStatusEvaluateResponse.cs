using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG34_AreaStatusEvaluate
{
    public class AreaStatusEvaluateResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusDto? Data { get; set; }
        public AreaStatusCode StatusCode { get; set; }
    }
}

