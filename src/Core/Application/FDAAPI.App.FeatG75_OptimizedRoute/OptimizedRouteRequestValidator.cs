using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace FDAAPI.App.FeatG75_OptimizedRoute
{
    public class OptimizedRouteRequestValidator : AbstractValidator<OptimizedRouteRequest>
    {
        public OptimizedRouteRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.StartLatitude)
                .InclusiveBetween(-90, 90).WithMessage("Start latitude must be between -90 and 90");

            RuleFor(x => x.StartLongitude)
                .InclusiveBetween(-180, 180).WithMessage("Start longitude must be between -180 and 180");

            RuleFor(x => x.EndLatitude)
                .InclusiveBetween(-90, 90).WithMessage("End latitude must be between -90 and 90");

            RuleFor(x => x.EndLongitude)
                .InclusiveBetween(-180, 180).WithMessage("End longitude must be between -180 and 180");

            RuleFor(x => x.RouteProfile)
                .NotEmpty().WithMessage("Route profile is required")
                .Must(p => p == "car" || p == "bike" || p == "foot")
                .WithMessage("Route profile must be 'car', 'bike', or 'foot'");

            RuleFor(x => x.MaxAlternatives)
                .InclusiveBetween(0, 5).WithMessage("Max alternatives must be between 0 and 5");

            RuleFor(x => x.Waypoints)
                .Must(w => w == null || w.Count <= 5)
                .WithMessage("Maximum 5 waypoints allowed");

            RuleForEach(x => x.Waypoints).ChildRules(wp =>
            {
                wp.RuleFor(w => w.Latitude)
                    .InclusiveBetween(-90, 90).WithMessage("Waypoint latitude must be between -90 and 90");
                wp.RuleFor(w => w.Longitude)
                    .InclusiveBetween(-180, 180).WithMessage("Waypoint longitude must be between -180 and 180");
            });

            RuleFor(x => x.DepartureTime)
                .Must(dt => !dt.HasValue || dt.Value > DateTime.UtcNow)
                .WithMessage("Departure time must be in the future")
                .Must(dt => !dt.HasValue || dt.Value <= DateTime.UtcNow.AddHours(24))
                .WithMessage("Departure time must be within the next 24 hours");
        }
    }
}
