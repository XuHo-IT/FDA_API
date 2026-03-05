using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Metadata for flood history response
    /// </summary>
    public class FloodHistoryMetadataDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Granularity { get; set; } = "hourly";
        public int TotalDataPoints { get; set; }
        public int MissingIntervals { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
