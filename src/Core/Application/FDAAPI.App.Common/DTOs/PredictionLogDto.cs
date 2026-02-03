using System;

namespace FDAAPI.App.Common.DTOs
{
    public class PredictionLogDto
    {
        public Guid PredictionLogId { get; set; }
        public Guid? AreaId { get; set; }  // Nullable: for user-created Areas
        public Guid? AdministrativeAreaId { get; set; }  // Nullable: for AdministrativeArea predictions
        public string? AreaName { get; set; }
        public string? AdministrativeAreaName { get; set; }
        public decimal PredictedProb { get; set; }
        public decimal? AiProb { get; set; }
        public decimal? PhysicsProb { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? ActualWaterLevel { get; set; }
        public bool IsVerified { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal? AccuracyScore { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PredictionComparisonSummaryDto
    {
        public int TotalPredictions { get; set; }
        public int VerifiedCount { get; set; }
        public int CorrectCount { get; set; }
        public decimal AccuracyRate { get; set; }
        public decimal AvgAccuracyScore { get; set; }
    }

    public class PredictionAccuracyStatsDto
    {
        public PredictionPeriodDto Period { get; set; } = new();
        public OverallPredictionStatsDto Overall { get; set; } = new();
        public List<PeriodPredictionStatsDto> ByPeriod { get; set; } = new();
        public List<AreaPredictionStatsDto> ByArea { get; set; } = new();
    }

    public class PredictionPeriodDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class OverallPredictionStatsDto
    {
        public int TotalPredictions { get; set; }
        public int VerifiedCount { get; set; }
        public int CorrectCount { get; set; }
        public decimal AccuracyRate { get; set; }
        public decimal AvgAccuracyScore { get; set; }
    }

    public class PeriodPredictionStatsDto
    {
        public string Period { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Correct { get; set; }
        public decimal AccuracyRate { get; set; }
        public decimal AvgAccuracyScore { get; set; }
    }

    public class AreaPredictionStatsDto
    {
        public Guid AreaId { get; set; }
        public string? AreaName { get; set; }
        public int Total { get; set; }
        public int Correct { get; set; }
        public decimal AccuracyRate { get; set; }
    }

    public class LogPredictionDataDto
    {
        public Guid PredictionLogId { get; set; }
        public Guid? AreaId { get; set; }  // Nullable: not applicable for AdministrativeArea predictions
        public Guid? AdministrativeAreaId { get; set; }  // Nullable: not applicable for Area predictions
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

