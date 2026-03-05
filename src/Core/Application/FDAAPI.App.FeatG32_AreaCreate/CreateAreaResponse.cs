using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Areas;
using System;

namespace FDAAPI.App.FeatG32_AreaCreate
{
    public class CreateAreaResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusCode StatusCode { get; set; }
        public AreaDto? Data { get; set; }
    }
}
