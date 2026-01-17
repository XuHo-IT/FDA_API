using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Single data point for flood history charts
    /// </summary>
    public class FloodDataPointDto
    {
        /// <summary>
        /// Timestamp of the reading/aggregation
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Water level value in centimeters
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Water level value in meters (for display)
        /// </summary>
        public double? ValueMeters { get; set; }

        /// <summary>
        /// Data quality flag: ok, suspect, bad
        /// </summary>
        public string? QualityFlag { get; set; }

        /// <summary>
        /// Flood severity: safe, caution, warning, critical
        /// </summary>
        public string Severity { get; set; } = "safe";

        /// <summary>
        /// Severity level: 0=safe, 1=caution, 2=warning, 3=critical
        /// </summary>
        public int SeverityLevel { get; set; }
    }
}
