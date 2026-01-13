using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class AreaStatusDto
    {
        public Guid AreaId { get; set; }
        public string Status { get; set; } = "Unknown";
        public int SeverityLevel { get; set; } = -1;
        public string Summary { get; set; } = string.Empty;
        public List<ContributingStationDto> ContributingStations { get; set; } = new();
        public DateTime EvaluatedAt { get; set; }
    }
}
