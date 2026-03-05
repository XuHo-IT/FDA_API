using FluentValidation;

namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer
{
    public class GetFloodSeverityLayerRequestValidator : AbstractValidator<GetFloodSeverityLayerRequest>
    {
        public GetFloodSeverityLayerRequestValidator()
        {
            // Bounds and ZoomLevel are optional, but if ZoomLevel is provided, validate range
            When(x => x.ZoomLevel.HasValue, () =>
            {
                RuleFor(x => x.ZoomLevel)
                    .InclusiveBetween(1, 20).WithMessage("Zoom level must be between 1 and 20.");
            });
        }
    }
}

