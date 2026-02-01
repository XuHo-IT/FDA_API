using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG76_LogPrediction
{
    public sealed record LogPredictionRequest(
        Guid AreaId,
        decimal PredictedProb,
        decimal? AiProb,
        decimal? PhysicsProb,
        string RiskLevel,
        DateTime StartTime,
        DateTime EndTime
    ) : IFeatureRequest<LogPredictionResponse>;
}

