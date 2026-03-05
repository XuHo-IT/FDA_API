using FluentValidation;

namespace FDAAPI.App.FeatG2_SensorReadingUpdate
{
    public class UpdateSensorReadingRequestValidator : AbstractValidator<UpdateSensorReadingRequest>
    {
        public UpdateSensorReadingRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Sensor reading ID is required.");

            RuleFor(x => x.StationId)
                .NotEmpty().WithMessage("Station ID is required.");

            RuleFor(x => x.Value)
                .GreaterThan(0).WithMessage("Value must be greater than 0.");

            RuleFor(x => x.Distance)
                .GreaterThanOrEqualTo(0).WithMessage("Distance must be greater than or equal to 0.");

            RuleFor(x => x.SensorHeight)
                .GreaterThan(0).WithMessage("Sensor height must be greater than 0.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit is required.")
                .MaximumLength(10).WithMessage("Unit must not exceed 10 characters.");

            RuleFor(x => x.MeasuredAt)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Measured date cannot be in the future.");

            RuleFor(x => x.UpdatedByUserId)
                .NotEmpty().WithMessage("Updated by user ID is required.");
        }
    }
}

