using FluentValidation;

namespace FDAAPI.App.FeatG3_SensorReadingGet
{
    public class GetSensorReadingRequestValidator : AbstractValidator<GetSensorReadingRequest>
    {
        public GetSensorReadingRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Sensor reading ID is required.");
        }
    }
}

