using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Response DTO for flood history endpoint
    /// </summary>
    public class FloodHistoryDto
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public List<FloodDataPointDto> DataPoints { get; set; } = new();
        public FloodHistoryMetadataDto Metadata { get; set; } = new();
    }
}
