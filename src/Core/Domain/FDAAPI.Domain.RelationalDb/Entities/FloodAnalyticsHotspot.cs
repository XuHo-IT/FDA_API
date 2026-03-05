using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class FloodAnalyticsHotspot : EntityWithId<Guid>
    {
        public Guid AdministrativeAreaId { get; set; }
        public decimal Score { get; set; }
        public int? Rank { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime CalculatedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual AdministrativeArea? AdministrativeArea { get; set; }
    }
}

