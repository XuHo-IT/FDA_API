using FluentValidation;

namespace FDAAPI.App.FeatG76_LogPrediction
{
    public class LogPredictionRequestValidator : AbstractValidator<LogPredictionRequest>
    {
        public LogPredictionRequestValidator()
        {
            RuleFor(x => x.AreaId)
                .NotEmpty().WithMessage("Area ID is required.");

            RuleFor(x => x.PredictedProb)
                .InclusiveBetween(0, 1).WithMessage("Predicted probability must be between 0 and 1.");

            RuleFor(x => x.AiProb)
                .InclusiveBetween(0, 1).WithMessage("AI probability must be between 0 and 1.")
                .When(x => x.AiProb.HasValue);

            RuleFor(x => x.PhysicsProb)
                .InclusiveBetween(0, 1).WithMessage("Physics probability must be between 0 and 1.")
                .When(x => x.PhysicsProb.HasValue);

            RuleFor(x => x.RiskLevel)
                .NotEmpty().WithMessage("Risk level is required.")
                .Must(level => new[] { "low", "medium", "high", "critical" }.Contains(level.ToLower()))
                .WithMessage("Risk level must be one of: low, medium, high, critical.")
                .When(x => !string.IsNullOrWhiteSpace(x.RiskLevel));

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Start time is required.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("End time is required.")
                .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
        }
    }
}

