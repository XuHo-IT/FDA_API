using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    public class PredictionLog : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        public Guid AreaId { get; set; }
        public decimal PredictedProb { get; set; }  // 0.0000 - 1.0000
        public decimal? AiProb { get; set; }
        public decimal? PhysicsProb { get; set; }
        public string RiskLevel { get; set; } = string.Empty;  // low, medium, high, critical
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        // Verification results
        public decimal? ActualWaterLevel { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool? IsCorrect { get; set; }
        public decimal? AccuracyScore { get; set; }
        public DateTime? VerifiedAt { get; set; }
        
        // Audit fields
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation
        [JsonIgnore]
        public virtual Area? Area { get; set; }
    }
}

