using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities.Base;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    /// <summary>
    /// Stores flexible user settings using JSONB (PostgreSQL)
    /// </summary>
    public class UserPreference : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        /// <summary>
        /// Foreign key to User
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Setting category: "map_layers", "notifications", "theme", etc.
        /// </summary>
        public string PreferenceKey { get; set; } = string.Empty;

        /// <summary>
        /// JSON string with setting values (stored as JSONB in PostgreSQL)
        /// </summary>
        public string PreferenceValue { get; set; } = "{}";

        // Audit fields (from interfaces)
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}
