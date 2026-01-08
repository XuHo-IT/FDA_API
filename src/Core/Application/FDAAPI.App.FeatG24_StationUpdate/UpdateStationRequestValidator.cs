using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG24_StationUpdate
{
    public class UpdateStationRequestValidator : AbstractValidator<UpdateStationRequest>
    {
        public UpdateStationRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Station ID is required.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("The station code must not be left blank.")
                .MaximumLength(50).WithMessage("Station code must not exceed 50 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The station name must not be left blank.")
                .MaximumLength(255);

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("The latitude must be between -90 and 90 degrees.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("The longitude must be between -180 and 180.");

            RuleFor(x => x.Status)
                .Must(s => new[] { "active", "offline", "maintenance" }.Contains(s))
                .WithMessage("Invalid status");
        }
    }
}

