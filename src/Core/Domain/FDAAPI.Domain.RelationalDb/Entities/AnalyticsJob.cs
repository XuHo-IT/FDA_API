using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class AnalyticsJob : EntityWithId<Guid>
    {
        public string JobType { get; set; } = string.Empty; // "FREQUENCY_AGG", "SEVERITY_AGG", "HOTSPOT_AGG"
        public string? Schedule { get; set; } // Cron expression or schedule description
        public bool IsActive { get; set; } = true;
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

