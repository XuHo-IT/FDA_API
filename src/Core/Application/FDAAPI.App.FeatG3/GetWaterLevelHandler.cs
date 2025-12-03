using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.Feat3
{
    /// <summary>
    /// Handler for retrieving a water level record
    /// Implements the business logic for fetching water level measurements
    /// </summary>
    public class GetWaterLevelHandler : IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse>
    {
        private readonly IWaterLevelRepository _waterLevelRepository;

        public GetWaterLevelHandler(IWaterLevelRepository waterLevelRepository)
        {
            _waterLevelRepository = waterLevelRepository;
        }

        public async Task<GetWaterLevelResponse> ExecuteAsync(GetWaterLevelRequest request, CancellationToken ct)
        {
            // Validation: Check if request is valid
            if (request.WaterLevelId <= 0)
            {
                return new GetWaterLevelResponse
                {
                    Success = false,
                    Message = "Invalid water level ID"
                };
            }

            try
            {
                var waterLevel = await _waterLevelRepository.GetByIdAsync(request.WaterLevelId);

                if (waterLevel == null)
                {
                    return new GetWaterLevelResponse
                    {
                        Success = false,
                        Message = $"Water level with ID {request.WaterLevelId} not found"
                    };
                }

                return new GetWaterLevelResponse
                {
                    Success = true,
                    Message = "Water level retrieved successfully",
                    WaterLevelId = waterLevel.Id,
                    LocationName = waterLevel.LocationName,
                    WaterLevel = waterLevel.Value,
                    Unit = waterLevel.Unit,
                    MeasuredAt = waterLevel.MeasuredAt
                };
            }
            catch (Exception ex)
            {
                return new GetWaterLevelResponse
                {
                    Success = false,
                    Message = $"Error retrieving water level: {ex.Message}"
                };
            }
        }
    }
}
