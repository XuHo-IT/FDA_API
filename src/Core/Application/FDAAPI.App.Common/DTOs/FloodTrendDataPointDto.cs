using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Aggregated data point for trend analysis
    /// </summary>
    public class FloodTrendDataPointDto
    {
        /// <summary>
        /// Period label: "2026-01-16", "2026-W03", "2026-01"
        /// </summary>
        public string Period { get; set; } = string.Empty;

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Maximum water level in period (cm)
        /// </summary>
        public double MaxLevel { get; set; }

        /// <summary>
        /// Minimum water level in period (cm)
        /// </summary>
        public double MinLevel { get; set; }

        /// <summary>
        /// Average water level in period (cm)
        /// </summary>
        public double AvgLevel { get; set; }

        /// <summary>
        /// Number of readings in this period
        /// </summary>
        public int ReadingCount { get; set; }

        /// <summary>
        /// Hours with flood conditions in this period
        /// </summary>
        public int FloodHours { get; set; }

        /// <summary>
        /// Total rainfall in mm (if available)
        /// </summary>
        public double? RainfallTotal { get; set; }

        /// <summary>
        /// Highest severity level reached: safe, caution, warning, critical
        /// </summary>
        public string PeakSeverity { get; set; } = "safe";
    }
}
