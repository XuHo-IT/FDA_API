using FluentValidation;

namespace FDAAPI.App.FeatG47_FrequencyAggregation
{
    public class FrequencyAggregationRequestValidator : AbstractValidator<FrequencyAggregationRequest>
    {
        public FrequencyAggregationRequestValidator()
        {
            RuleFor(x => x.BucketType)
                .NotEmpty().WithMessage("Bucket type is required.")
                .Must(BeValidBucketType).WithMessage("Bucket type must be 'day', 'week', 'month', or 'year'.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

            RuleFor(x => x.EndDate)
                .Must((request, endDate) => (endDate - request.StartDate).TotalDays <= 365)
                .WithMessage("Time range cannot exceed 365 days.");
        }

        private bool BeValidBucketType(string bucketType)
        {
            return bucketType?.ToLower() is "day" or "week" or "month" or "year";
        }
    }
}

