using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG23_StationCreate
{
    public class CreateStationResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StationStatusCode StatusCode { get; set; }
        public Guid? StationId { get; set; }
    }
}
