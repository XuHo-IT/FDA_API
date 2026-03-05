using FDAAPI.Domain.RelationalDb.Entities.Base;
using FDAAPI.Domain.RelationalDb.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class NotificationLog : EntityWithId<Guid>
    {
        public Guid UserId { get; set; }
        public Guid AlertId { get; set; }
        public NotificationChannel Channel { get; set; }
        public string Destination { get; set; } = string.Empty; // phone/email/device_token
        public string Content { get; set; } = string.Empty;
        public string? Title { get; set; }
        public NotificationPriority Priority { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public string Status { get; set; } = "pending"; // pending, sent, failed, delivered
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(AlertId))]
        [JsonIgnore]
        public virtual Alert? Alert { get; set; }
    }
}