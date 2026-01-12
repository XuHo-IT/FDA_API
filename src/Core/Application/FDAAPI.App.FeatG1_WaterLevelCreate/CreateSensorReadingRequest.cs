using FDAAPI.App.Common.Features;
using MediatR;

namespace FDAAPI.App.FeatG1_SensorReadingCreate
{
    public record CreateSensorReadingRequest(
        Guid StationId,
        double Value,
        double Distance,
        double SensorHeight,
        string Unit,
        int Status,
        DateTime MeasuredAt,
        Guid CreatedByUserId
    ) : IFeatureRequest<CreateSensorReadingResponse>;
}