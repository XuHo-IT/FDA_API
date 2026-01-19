using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.AdministrativeAreas;

namespace FDAAPI.App.FeatG59_AdministrativeAreaGet
{
    public class GetAdministrativeAreaResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdministrativeAreaStatusCode StatusCode { get; set; }
        public AdministrativeAreaDto? AdministrativeArea { get; set; }
    }
}

