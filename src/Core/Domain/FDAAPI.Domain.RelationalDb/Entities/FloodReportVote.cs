using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class FloodReportVote : EntityWithId<Guid>
    {
        public Guid FloodReportId { get; set; }
        public Guid UserId { get; set; }
        public string VoteType { get; set; } = "up"; // up | down
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public virtual FloodReport? FloodReport { get; set; }
        
        [JsonIgnore]
        public virtual User? User { get; set; }
    }
}

