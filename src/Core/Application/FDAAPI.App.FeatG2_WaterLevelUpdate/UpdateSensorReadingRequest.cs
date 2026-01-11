using FDAAPI.App.Common.Features;
using MediatR;

namespace FDAAPI.App.FeatG2_SensorReadingUpdate
{
    public record UpdateSensorReadingRequest(
        Guid Id,
        Guid StationId,
        double Value,
        double Distance,
        double SensorHeight,
        string Unit,
        int Status,
        DateTime MeasuredAt,
        Guid UpdatedByUserId
    ) : IFeatureRequest<UpdateSensorReadingResponse>;
}