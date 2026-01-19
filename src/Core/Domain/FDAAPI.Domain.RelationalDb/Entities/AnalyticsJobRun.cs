using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class AnalyticsJobRun : EntityWithId<Guid>
    {
        public Guid JobId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string Status { get; set; } = string.Empty; // "RUNNING", "SUCCESS", "FAILED", "CANCELLED"
        public string? ErrorMessage { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsCreated { get; set; }
        public int? ExecutionTimeMs { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual AnalyticsJob? Job { get; set; }
    }
}

