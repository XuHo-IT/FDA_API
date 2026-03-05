using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Summary statistics for the trend period
    /// </summary>
    public class FloodTrendSummaryDto
    {
        public int TotalFloodHours { get; set; }
        public double AvgWaterLevel { get; set; }
        public double MaxWaterLevel { get; set; }
        public double MinWaterLevel { get; set; }
        public int DaysWithFlooding { get; set; }
        public string? MostAffectedDay { get; set; }
    }
}
