using FluentValidation;

namespace FDAAPI.App.FeatG31_GetMapCurrentStatus
{
    public class GetMapCurrentStatusRequestValidator : AbstractValidator<GetMapCurrentStatusRequest>
    {
        public GetMapCurrentStatusRequestValidator()
        {
            // Validate latitude ranges if provided
            When(x => x.MinLat.HasValue, () =>
            {
                RuleFor(x => x.MinLat)
                    .InclusiveBetween(-90, 90).WithMessage("Minimum latitude must be between -90 and 90 degrees.");
            });

            When(x => x.MaxLat.HasValue, () =>
            {
                RuleFor(x => x.MaxLat)
                    .InclusiveBetween(-90, 90).WithMessage("Maximum latitude must be between -90 and 90 degrees.");
            });

            // Validate longitude ranges if provided
            When(x => x.MinLng.HasValue, () =>
            {
                RuleFor(x => x.MinLng)
                    .InclusiveBetween(-180, 180).WithMessage("Minimum longitude must be between -180 and 180 degrees.");
            });

            When(x => x.MaxLng.HasValue, () =>
            {
                RuleFor(x => x.MaxLng)
                    .InclusiveBetween(-180, 180).WithMessage("Maximum longitude must be between -180 and 180 degrees.");
            });

            // Validate status if provided
            When(x => !string.IsNullOrEmpty(x.Status), () =>
            {
                RuleFor(x => x.Status)
                    .Must(s => new[] { "active", "offline", "maintenance" }.Contains(s))
                    .WithMessage("Status must be one of: active, offline, maintenance.");
            });

            // Validate that MinLat <= MaxLat and MinLng <= MaxLng
            When(x => x.MinLat.HasValue && x.MaxLat.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.MinLat <= x.MaxLat)
                    .WithMessage("Minimum latitude must be less than or equal to maximum latitude.");
            });

            When(x => x.MinLng.HasValue && x.MaxLng.HasValue, () =>
            {
                RuleFor(x => x)
                    .Must(x => x.MinLng <= x.MaxLng)
                    .WithMessage("Minimum longitude must be less than or equal to maximum longitude.");
            });
        }
    }
}

