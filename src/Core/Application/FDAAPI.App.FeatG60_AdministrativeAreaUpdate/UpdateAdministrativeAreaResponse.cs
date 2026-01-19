using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.AdministrativeAreas;

namespace FDAAPI.App.FeatG60_AdministrativeAreaUpdate
{
    public class UpdateAdministrativeAreaResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdministrativeAreaStatusCode StatusCode { get; set; }
    }
}

