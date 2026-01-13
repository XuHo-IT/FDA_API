using FluentValidation;

namespace FDAAPI.App.FeatG26_StationGet
{
    public class GetStationRequestValidator : AbstractValidator<GetStationRequest>
    {
        public GetStationRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Station ID is required.");
        }
    }
}

