using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class FloodReportMedia : EntityWithId<Guid>
    {
        public Guid FloodReportId { get; set; }
        public string MediaType { get; set; } = "photo"; // photo | video
        public string MediaUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        [JsonIgnore]
        public virtual FloodReport? FloodReport { get; set; }
    }
}

