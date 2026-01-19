using FluentValidation;

namespace FDAAPI.App.FeatG52_GetSeverityAnalytics
{
    public class GetSeverityAnalyticsRequestValidator : AbstractValidator<GetSeverityAnalyticsRequest>
    {
        public GetSeverityAnalyticsRequestValidator()
        {
            RuleFor(x => x.BucketType)
                .Must(BeValidBucketType).WithMessage("Bucket type must be 'day', 'week', 'month', or 'year'.")
                .When(x => !string.IsNullOrEmpty(x.BucketType));

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        }

        private bool BeValidBucketType(string? bucketType)
        {
            return bucketType?.ToLower() is null or "day" or "week" or "month" or "year";
        }
    }
}

