using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.AdministrativeAreas;

namespace FDAAPI.App.FeatG57_AdministrativeAreaCreate
{
    public class CreateAdministrativeAreaResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdministrativeAreaStatusCode StatusCode { get; set; }
        public AdministrativeAreaDto? Data { get; set; }
    }
}

