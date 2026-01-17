using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Response DTO for flood trends endpoint
    /// </summary>
    public class FloodTrendDto
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Granularity { get; set; } = "daily";
        public List<FloodTrendDataPointDto> DataPoints { get; set; } = new();
        public FloodTrendComparisonDto? Comparison { get; set; }
        public FloodTrendSummaryDto Summary { get; set; } = new();
    }
}
