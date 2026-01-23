using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class ContributingStationDto
    {
        public Guid StationId { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public double Distance { get; set; }
        public double WaterLevel { get; set; }
        public string Severity { get; set; } = string.Empty;
        public int Weight { get; set; }
        
        // Ward information (for district and city level)
        public ContributingWardInfoDto? Ward { get; set; }
        
        // District information (for city level)
        public ContributingDistrictInfoDto? District { get; set; }
    }
    
    public class ContributingWardInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }
    
    public class ContributingDistrictInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }
}
