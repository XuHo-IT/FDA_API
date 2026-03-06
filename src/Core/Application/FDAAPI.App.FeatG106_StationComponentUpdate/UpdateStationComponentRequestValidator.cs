using FluentValidation;

namespace FDAAPI.App.FeatG106_StationComponentUpdate
{
    public class UpdateStationComponentRequestValidator : AbstractValidator<UpdateStationComponentRequest>
    {
        public UpdateStationComponentRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.Status)
                .Must(s => string.IsNullOrWhiteSpace(s) || Domain.RelationalDb.Entities.StationComponentStatuses.All.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", Domain.RelationalDb.Entities.StationComponentStatuses.All)} (or null/empty).");

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
