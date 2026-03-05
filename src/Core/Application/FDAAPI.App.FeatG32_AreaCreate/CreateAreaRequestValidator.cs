using FluentValidation;

namespace FDAAPI.App.FeatG32_AreaCreate
{
    public class CreateAreaRequestValidator : AbstractValidator<CreateAreaRequest>
    {
        public CreateAreaRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Area name is required.")
                .MaximumLength(255).WithMessage("Area name must not exceed 255 characters.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.");

            RuleFor(x => x.RadiusMeters)
                .GreaterThanOrEqualTo(100).WithMessage("Radius must be at least 100 meters.")
                .LessThanOrEqualTo(150).WithMessage("Radius must not exceed 150 meters.");

            RuleFor(x => x.AddressText)
                .MaximumLength(500).WithMessage("Address text must not exceed 500 characters.");
        }
    }
}

