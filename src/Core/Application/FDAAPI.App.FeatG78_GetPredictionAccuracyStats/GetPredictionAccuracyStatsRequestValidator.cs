using FluentValidation;

namespace FDAAPI.App.FeatG78_GetPredictionAccuracyStats
{
    public class GetPredictionAccuracyStatsRequestValidator : AbstractValidator<GetPredictionAccuracyStatsRequest>
    {
        public GetPredictionAccuracyStatsRequestValidator()
        {
            RuleFor(x => x.GroupBy)
                .Must(g => new[] { "day", "week", "month" }.Contains(g.ToLower()))
                .WithMessage("GroupBy must be one of: day, week, month.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        }
    }
}

