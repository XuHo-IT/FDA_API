using FDAAPI.Domain.RelationalDb.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    /// <summary>
    /// Pre-computed hourly aggregation of sensor readings for efficient timeseries queries.
    /// Used for 24h-30d chart visualizations.
    /// </summary>
    public class SensorHourlyAgg : EntityWithId<Guid>
    {
        /// <summary>
        /// Reference to the monitoring station
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// Start of the hour (truncated to hour, e.g., 2026-01-16T14:00:00Z)
        /// </summary>
        public DateTime HourStart { get; set; }

        /// <summary>
        /// Maximum water level recorded in this hour (in cm)
        /// </summary>
        public decimal MaxLevel { get; set; }

        /// <summary>
        /// Minimum water level recorded in this hour (in cm)
        /// </summary>
        public decimal MinLevel { get; set; }

        /// <summary>
        /// Average water level for this hour (in cm)
        /// </summary>
        public decimal AvgLevel { get; set; }

        /// <summary>
        /// Number of sensor readings in this hour
        /// </summary>
        public int ReadingCount { get; set; }

        /// <summary>
        /// Data quality score (0-100): percentage of valid readings
        /// </summary>
        public decimal QualityScore { get; set; }

        /// <summary>
        /// Timestamp when this aggregation was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        [ForeignKey(nameof(StationId))]
        public virtual Station? Station { get; set; }
    }
}
