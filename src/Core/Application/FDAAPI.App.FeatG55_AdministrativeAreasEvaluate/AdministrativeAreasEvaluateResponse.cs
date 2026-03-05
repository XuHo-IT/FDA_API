using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Areas;
using System;

namespace FDAAPI.App.FeatG55_AdministrativeAreasEvaluate
{
    public class AdministrativeAreasEvaluateResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdministrativeAreaStatusDto? Data { get; set; }
        public AreaStatusCode StatusCode { get; set; }
    }
}

