using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG84_FloodReportGetNearby
{
    public sealed class GetNearbyFloodReportsResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
        public string ConsensusLevel { get; set; } = "none"; // none | low | moderate | strong
        public string ConsensusMessage { get; set; } = string.Empty;
        public List<NearbyFloodReportItem> Reports { get; set; } = new();
    }

    public sealed class NearbyFloodReportItem
    {
        public Guid Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Severity { get; set; } = "medium";
        public DateTime CreatedAt { get; set; }
        public double DistanceMeters { get; set; }
    }
}


