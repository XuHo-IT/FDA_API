using FluentValidation;

namespace FDAAPI.App.FeatG109_StationComponentGet
{
    public class GetStationComponentByIdRequestValidator : AbstractValidator<GetStationComponentByIdRequest>
    {
        public GetStationComponentByIdRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");
        }
    }
}
