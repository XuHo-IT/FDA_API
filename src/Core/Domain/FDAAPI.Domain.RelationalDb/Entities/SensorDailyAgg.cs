using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    /// <summary>
    /// Pre-computed daily aggregation of sensor readings for long-term trend analysis.
    /// Used for 30d-1y chart visualizations.
    /// </summary>
    public class SensorDailyAgg : EntityWithId<Guid>
    {
        public Guid StationId { get; set; }
        public DateOnly Date { get; set; }
        public decimal MaxLevel { get; set; }
        public decimal MinLevel { get; set; }
        public decimal AvgLevel { get; set; }
        public decimal? RainfallTotal { get; set; }
        public int ReadingCount { get; set; }
        public int FloodHours { get; set; }
        public int PeakSeverity { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(StationId))]
        public virtual Station? Station { get; set; }
    }
}
