using FluentValidation;

namespace FDAAPI.App.FeatG77_GetPredictionComparisons
{
    public class GetPredictionComparisonsRequestValidator : AbstractValidator<GetPredictionComparisonsRequest>
    {
        public GetPredictionComparisonsRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");

            RuleFor(x => x.Size)
                .GreaterThan(0).WithMessage("Size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Size must not exceed 100.");

            RuleFor(x => x.MinAccuracy)
                .InclusiveBetween(0, 1).WithMessage("Min accuracy must be between 0 and 1.")
                .When(x => x.MinAccuracy.HasValue);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        }
    }
}

