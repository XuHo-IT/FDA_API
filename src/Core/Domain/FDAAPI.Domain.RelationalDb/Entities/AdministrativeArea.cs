using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class AdministrativeArea : EntityWithId<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty; // "ward", "district", "city"
        public Guid? ParentId { get; set; } // For hierarchical structure (ward -> district -> city)
        public string? Code { get; set; } // Administrative code (e.g., "DIST_01", "WARD_001")
        public string? Geometry { get; set; } // Optional: PostGIS geometry (POLYGON) as JSON string

        // Navigation properties
        [JsonIgnore]
        public virtual AdministrativeArea? Parent { get; set; }

        [JsonIgnore]
        public virtual ICollection<AdministrativeArea> Children { get; set; } = new List<AdministrativeArea>();

        [JsonIgnore]
        public virtual ICollection<Station> Stations { get; set; } = new List<Station>();
    }
}

