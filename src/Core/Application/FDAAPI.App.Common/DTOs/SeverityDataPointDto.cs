using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class SeverityDataPointDto
    {
        public DateTime TimeBucket { get; set; }
        public decimal? MaxLevel { get; set; }
        public decimal? AvgLevel { get; set; }
        public decimal? MinLevel { get; set; }
        public int DurationHours { get; set; }
        public int ReadingCount { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
