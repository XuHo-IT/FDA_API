using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Response DTO for flood statistics endpoint
    /// </summary>
    public class FloodStatisticsDto
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public FloodStatisticsSummaryDto Summary { get; set; } = new();
        public FloodStatisticsSeverityBreakdownDto? SeverityBreakdown { get; set; }
        public FloodTrendComparisonDto? Comparison { get; set; }
        public FloodDataQualityDto DataQuality { get; set; } = new();
    }

    /// <summary>
    /// Summary statistics
    /// </summary>
    public class FloodStatisticsSummaryDto
    {
        public double MaxWaterLevel { get; set; }
        public double MinWaterLevel { get; set; }
        public double AvgWaterLevel { get; set; }
        public int TotalFloodHours { get; set; }
        public int TotalReadings { get; set; }
        public int MissingIntervals { get; set; }
    }
}
