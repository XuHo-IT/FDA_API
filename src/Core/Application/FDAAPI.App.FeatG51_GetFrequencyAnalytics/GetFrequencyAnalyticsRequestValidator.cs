using FluentValidation;

namespace FDAAPI.App.FeatG51_GetFrequencyAnalytics
{
    public class GetFrequencyAnalyticsRequestValidator : AbstractValidator<GetFrequencyAnalyticsRequest>
    {
        public GetFrequencyAnalyticsRequestValidator()
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

