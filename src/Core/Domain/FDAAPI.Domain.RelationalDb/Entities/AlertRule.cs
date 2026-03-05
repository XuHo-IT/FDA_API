// File: FDA_API/src/Core/Domain/FDAAPI.Domain.RelationalDb/Entities/AlertRule.cs

using FDAAPI.Domain.RelationalDb.Entities.Base;
using FDAAPI.Domain.RelationalDb.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class AlertRule : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        public Guid StationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RuleType { get; set; } = "threshold"; // threshold, rate_change, duration
        public decimal ThresholdValue { get; set; }
        public int? DurationMin { get; set; } // Optional: must exceed threshold for X minutes
        public string Severity { get; set; } = "warning"; // info, caution, warning, critical
        public bool IsActive { get; set; } = true;
        public bool IsGlobalDefault { get; set; } = false;
        public SubscriptionTier MinTierRequired { get; set; } = SubscriptionTier.Free;
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(StationId))]
        [JsonIgnore]
        public virtual Station? Station { get; set; }

        [JsonIgnore]
        public virtual ICollection<Alert>? Alerts { get; set; }
    }
}