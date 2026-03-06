using FDAAPI.Domain.RelationalDb.Entities.Base;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class SensorIncident : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        public Guid StationId { get; set; }
        public string IncidentType { get; set; } = string.Empty; // hardware_fault, tampering, maintenance, offline
        public string Status { get; set; } = "open"; // open, in_progress, resolved, closed
        public string Priority { get; set; } = "medium"; // low, medium, high, critical
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? AssignedTo { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; } // fixed, wont_fix, duplicate
        public string? ResolutionNotes { get; set; }

        // Audit fields
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Station? Station { get; set; }

        [JsonIgnore]
        public virtual User? AssignedUser { get; set; }
    }
}
