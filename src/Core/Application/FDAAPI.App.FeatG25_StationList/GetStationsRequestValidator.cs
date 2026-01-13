using FluentValidation;

namespace FDAAPI.App.FeatG25_StationList
{
    public class GetStationsRequestValidator : AbstractValidator<GetStationsRequest>
    {
        public GetStationsRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

            When(x => !string.IsNullOrEmpty(x.SearchTerm), () =>
            {
                RuleFor(x => x.SearchTerm)
                    .MaximumLength(100).WithMessage("Search term must not exceed 100 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.Status), () =>
            {
                RuleFor(x => x.Status)
                    .Must(s => new[] { "active", "offline", "maintenance" }.Contains(s))
                    .WithMessage("Status must be one of: active, offline, maintenance.");
            });
        }
    }
}

