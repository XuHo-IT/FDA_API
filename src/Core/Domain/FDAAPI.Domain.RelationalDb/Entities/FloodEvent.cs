using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class FloodEvent : EntityWithId<Guid>
    {
        public Guid AdministrativeAreaId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? PeakLevel { get; set; }
        public int? DurationHours { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual AdministrativeArea? AdministrativeArea { get; set; }
    }
}

