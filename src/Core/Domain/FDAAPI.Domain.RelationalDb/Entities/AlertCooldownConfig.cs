using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class AlertCooldownConfig : EntityWithId<Guid>
    {
        public string Severity { get; set; } = "warning"; //info, caution, warning, critical
        public int CooldownMinutes { get; set; } = 10;
        public int MaxNotificationsPerHour { get; set; } = 6;
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}