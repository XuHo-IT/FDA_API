using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.AdministrativeAreas;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG58_AdministrativeAreaList
{
    public class GetAdministrativeAreasResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdministrativeAreaStatusCode StatusCode { get; set; }
        public List<AdministrativeAreaDto> AdministrativeAreas { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

