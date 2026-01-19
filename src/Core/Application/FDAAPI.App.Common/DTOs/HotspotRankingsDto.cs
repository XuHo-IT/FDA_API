using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class HotspotRankingsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string AreaLevel { get; set; } = string.Empty;
        public List<HotspotDto> Hotspots { get; set; } = new();
    }
}
