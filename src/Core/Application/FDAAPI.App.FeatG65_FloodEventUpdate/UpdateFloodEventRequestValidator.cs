using FluentValidation;

namespace FDAAPI.App.FeatG65_FloodEventUpdate
{
    public class UpdateFloodEventRequestValidator : AbstractValidator<UpdateFloodEventRequest>
    {
        public UpdateFloodEventRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID is required.");

            RuleFor(x => x.AdministrativeAreaId)
                .NotEmpty().WithMessage("Administrative area ID is required.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Start time is required.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("End time is required.")
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time.");

            RuleFor(x => x.PeakLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Peak level must be greater than or equal to 0.")
                .When(x => x.PeakLevel.HasValue);

            RuleFor(x => x.DurationHours)
                .GreaterThan(0).WithMessage("Duration hours must be greater than 0.")
                .When(x => x.DurationHours.HasValue);

            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required.");
        }
    }
}

