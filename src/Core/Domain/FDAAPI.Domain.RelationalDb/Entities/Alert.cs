using FDAAPI.Domain.RelationalDb.Entities.Base;
using FDAAPI.Domain.RelationalDb.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class Alert : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        public Guid? AlertRuleId { get; set; }
        public Guid StationId { get; set; }
        public DateTime TriggeredAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string Status { get; set; } = "open"; // open, resolved
        public string Severity { get; set; } = "info"; // info, caution, warning, critical
        public NotificationPriority Priority { get; set; } = NotificationPriority.Low;
        public decimal CurrentValue { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool NotificationSent { get; set; } = false;
        public int NotificationCount { get; set; } = 0;
        public DateTime? LastNotificationAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(AlertRuleId))]
        [JsonIgnore]
        public virtual AlertRule? AlertRule { get; set; }

        [ForeignKey(nameof(StationId))]
        [JsonIgnore]
        public virtual Station? Station { get; set; }

        [JsonIgnore]
        public virtual ICollection<NotificationLog>? NotificationLogs { get; set; }
    }
}