using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Comparison with previous period
    /// </summary>
    public class FloodTrendComparisonDto
    {
        public DateTime PreviousPeriodStart { get; set; }
        public DateTime PreviousPeriodEnd { get; set; }

        /// <summary>
        /// Change in average level (percentage, +/-)
        /// </summary>
        public double? AvgLevelChange { get; set; }

        /// <summary>
        /// Change in flood hours (percentage, +/-)
        /// </summary>
        public double? FloodHoursChange { get; set; }

        /// <summary>
        /// Change in peak level (percentage, +/-)
        /// </summary>
        public double? PeakLevelChange { get; set; }
    }
}
