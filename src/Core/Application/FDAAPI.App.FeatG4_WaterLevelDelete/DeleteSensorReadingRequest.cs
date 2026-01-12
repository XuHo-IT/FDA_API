using FDAAPI.App.Common.Features;
using MediatR;

namespace FDAAPI.App.FeatG4_SensorReadingDelete
{
    public record DeleteSensorReadingRequest(
        Guid Id,
        Guid DeletedByUserId  // For audit trail
    ) : IFeatureRequest<DeleteSensorReadingResponse>;
}