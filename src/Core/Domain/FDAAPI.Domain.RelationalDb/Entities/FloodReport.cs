using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class FloodReport : EntityWithId<Guid>
    {
        // Reporter (nullable for anonymous)
        public Guid? ReporterUserId { get; set; }
        
        // Location
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
        
        // Content
        public string? Description { get; set; }
        public string Severity { get; set; } = "medium"; // low | medium | high
        
        // Trust Score & Status
        public int TrustScore { get; set; } = 50; // 0-100
        public string Status { get; set; } = "published"; // published | hidden | escalated
        public string ConfidenceLevel { get; set; } = "medium"; // low | medium | high
        public string Priority { get; set; } = "normal"; // normal | high | critical
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public virtual User? Reporter { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<FloodReportMedia> Media { get; set; } = new List<FloodReportMedia>();
        
        [JsonIgnore]
        public virtual ICollection<FloodReportVote> Votes { get; set; } = new List<FloodReportVote>();
        
        [JsonIgnore]
        public virtual ICollection<FloodReportFlag> Flags { get; set; } = new List<FloodReportFlag>();
    }
}

