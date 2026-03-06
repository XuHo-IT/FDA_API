using FluentValidation;

namespace FDAAPI.App.FeatG107_StationComponentDelete
{
    public class DeleteStationComponentRequestValidator : AbstractValidator<DeleteStationComponentRequest>
    {
        public DeleteStationComponentRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
        }
    }
}
