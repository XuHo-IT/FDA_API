using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Data quality information
    /// </summary>
    public class FloodDataQualityDto
    {
        /// <summary>
        /// Percentage of expected data points that exist (0-100)
        /// </summary>
        public double Completeness { get; set; }

        /// <summary>
        /// List of detected data gaps
        /// </summary>
        public List<MissingIntervalDto> MissingIntervals { get; set; } = new();
    }
}
