using FluentValidation;

namespace FDAAPI.App.FeatG105_StationComponentCreate
{
    public class CreateStationComponentRequestValidator : AbstractValidator<CreateStationComponentRequest>
    {
        public CreateStationComponentRequestValidator()
        {
            RuleFor(x => x.StationId)
                .NotEmpty().WithMessage("StationId is required.");

            RuleFor(x => x.ComponentType)
                .NotEmpty().WithMessage("ComponentType is required.")
                .Must(t => Domain.RelationalDb.Entities.StationComponentTypes.All.Contains(t))
                .WithMessage($"ComponentType must be one of: {string.Join(", ", Domain.RelationalDb.Entities.StationComponentTypes.All)}");

            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Model)
                .MaximumLength(100).WithMessage("Model cannot exceed 100 characters.");

            RuleFor(x => x.SerialNumber)
                .MaximumLength(100).WithMessage("SerialNumber cannot exceed 100 characters.");

            RuleFor(x => x.FirmwareVersion)
                .MaximumLength(50).WithMessage("FirmwareVersion cannot exceed 50 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        }
    }
}
