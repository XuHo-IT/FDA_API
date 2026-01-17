using FDAAPI.Domain.RelationalDb.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class UserAlertSubscription : EntityWithId<Guid>
    {
        public Guid UserId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? StationId { get; set; }
        public string MinSeverity { get; set; } = "warning";
        public bool EnablePush { get; set; } = true;
        public bool EnableEmail { get; set; } = false;
        public bool EnableSms { get; set; } = false;
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(StationId))]
        [JsonIgnore]
        public virtual Station? Station { get; set; }

        [ForeignKey(nameof(AreaId))]
        [JsonIgnore]
        public virtual Area? Area { get; set; }
    }
}