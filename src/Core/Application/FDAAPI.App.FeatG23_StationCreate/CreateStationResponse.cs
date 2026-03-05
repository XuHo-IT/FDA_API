using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using System;

namespace FDAAPI.App.FeatG23_StationCreate
{
    public class CreateStationResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StationStatusCode StatusCode { get; set; }
        public StationDto? Data { get; set; }
    }
}
