using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.StaticData;

namespace FDAAPI.App.FeatG5;

public class GetStaticDataHandler : IFeatureHandler<GetStaticDataRequest, GetStaticDataResponse>
{
    public Task<GetStaticDataResponse> ExecuteAsync(GetStaticDataRequest request, CancellationToken ct)
    {
        return Task.FromResult(new GetStaticDataResponse
        {
            Success = true,
            Message = "Static data retrieved successfully",
            DanangCenter = RawData.DANANG_CENTER,
            MockSensors = RawData.MOCK_SENSORS,
            FloodZones = RawData.FLOOD_ZONES
        });
    }
}
