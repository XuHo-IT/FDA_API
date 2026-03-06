using FluentValidation;

namespace FDAAPI.App.FeatG108_StationComponentList
{
    public class GetStationComponentsRequestValidator : AbstractValidator<GetStationComponentsRequest>
    {
        public GetStationComponentsRequestValidator()
        {
            RuleFor(x => x.StationId)
                .NotEmpty().WithMessage("StationId is required.");
        }
    }
}
