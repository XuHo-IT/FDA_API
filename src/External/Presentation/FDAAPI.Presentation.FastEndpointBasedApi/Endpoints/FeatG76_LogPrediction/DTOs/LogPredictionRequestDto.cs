using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG76_LogPrediction.DTOs
{
    public class LogPredictionRequestDto
    {
        public Guid AdministrativeAreaId { get; set; }
        public decimal PredictedProb { get; set; }
        public decimal? AiProb { get; set; }
        public decimal? PhysicsProb { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

