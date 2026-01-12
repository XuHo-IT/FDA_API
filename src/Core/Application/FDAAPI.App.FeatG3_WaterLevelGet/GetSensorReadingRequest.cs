using FDAAPI.App.Common.Features;
using MediatR;

namespace FDAAPI.App.FeatG3_SensorReadingGet
{
    public record GetSensorReadingRequest(
        Guid Id
    ) : IFeatureRequest<GetSensorReadingResponse>;
}