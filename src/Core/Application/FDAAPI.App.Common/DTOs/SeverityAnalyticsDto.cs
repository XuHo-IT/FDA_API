using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class SeverityAnalyticsDto
    {
        public Guid? AdministrativeAreaId { get; set; }
        public string? AdministrativeAreaName { get; set; }
        public string BucketType { get; set; } = string.Empty;
        public List<SeverityDataPointDto> DataPoints { get; set; } = new();
    }
}
