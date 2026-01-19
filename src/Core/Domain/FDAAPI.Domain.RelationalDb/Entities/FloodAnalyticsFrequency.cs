using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class FloodAnalyticsFrequency : EntityWithId<Guid>
    {
        public Guid AdministrativeAreaId { get; set; }
        public DateTime TimeBucket { get; set; }
        public string BucketType { get; set; } = string.Empty; // "day", "week", "month", "year"
        public int EventCount { get; set; }
        public int ExceedCount { get; set; }
        public DateTime CalculatedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual AdministrativeArea? AdministrativeArea { get; set; }
    }
}

