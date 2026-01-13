using FluentValidation;

namespace FDAAPI.App.FeatG27_StationDelete
{
    public class DeleteStationRequestValidator : AbstractValidator<DeleteStationRequest>
    {
        public DeleteStationRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Station ID is required.");
        }
    }
}

