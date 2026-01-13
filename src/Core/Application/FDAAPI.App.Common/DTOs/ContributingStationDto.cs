using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class ContributingStationDto
    {
        public string StationCode { get; set; } = string.Empty;
        public double Distance { get; set; }
        public double WaterLevel { get; set; }
        public string Severity { get; set; } = string.Empty;
        public int Weight { get; set; }
    }
}
