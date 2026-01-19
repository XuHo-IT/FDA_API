using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class FloodAnalyticsSeverity : EntityWithId<Guid>
    {
        public Guid AdministrativeAreaId { get; set; }
        public DateTime TimeBucket { get; set; }
        public string BucketType { get; set; } = string.Empty; // "day", "week", "month", "year"
        public decimal? MaxLevel { get; set; }
        public decimal? AvgLevel { get; set; }
        public decimal? MinLevel { get; set; }
        public int DurationHours { get; set; }
        public int ReadingCount { get; set; }
        public DateTime CalculatedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual AdministrativeArea? AdministrativeArea { get; set; }
    }
}

