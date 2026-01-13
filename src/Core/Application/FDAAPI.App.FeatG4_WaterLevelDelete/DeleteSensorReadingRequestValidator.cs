using FluentValidation;

namespace FDAAPI.App.FeatG4_SensorReadingDelete
{
    public class DeleteSensorReadingRequestValidator : AbstractValidator<DeleteSensorReadingRequest>
    {
        public DeleteSensorReadingRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Sensor reading ID is required.");

            RuleFor(x => x.DeletedByUserId)
                .NotEmpty().WithMessage("Deleted by user ID is required.");
        }
    }
}

