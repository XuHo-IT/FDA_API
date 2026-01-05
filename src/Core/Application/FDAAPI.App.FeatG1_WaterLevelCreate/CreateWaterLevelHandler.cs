using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.FeatG1_WaterLevelCreate
{
    /// <summary>
    /// Handler for creating a water level record
    /// Implements the business logic for recording water level measurements
    /// </summary>
    public class CreateWaterLevelHandler : IFeatureHandler<CreateWaterLevelRequest, CreateWaterLevelResponse>
    {
        private readonly IWaterLevelRepository _waterLevelRepository;

        public CreateWaterLevelHandler(IWaterLevelRepository waterLevelRepository)
        {
            _waterLevelRepository = waterLevelRepository;
        }

        public async Task<CreateWaterLevelResponse> ExecuteAsync(CreateWaterLevelRequest request, CancellationToken ct)
        {
            // Validation: Check if request is valid
            if (string.IsNullOrWhiteSpace(request.LocationName))
            {
                return new CreateWaterLevelResponse
                {
                    Success = false,
                    Message = "Location name is required"
                };
            }

            if (request.WaterLevel < 0)
            {
                return new CreateWaterLevelResponse
                {
                    Success = false,
                    Message = "Water level cannot be negative"
                };
            }

            try
            {
                var waterLevel = new WaterLevel
                {
                    LocationName = request.LocationName,
                    Value = request.WaterLevel,
                    Unit = request.Unit,
                    MeasuredAt = request.MeasuredAt,
                    CreatedAt = DateTime.UtcNow
                };

                var createdWaterLevelId = await _waterLevelRepository.CreateAsync(waterLevel);

                return new CreateWaterLevelResponse
                {
                    Success = true,
                    Message = "Water level recorded successfully",
                    WaterLevelId = createdWaterLevelId,
                    LocationName = request.LocationName,
                    WaterLevel = request.WaterLevel,
                    CreatedAt = waterLevel.CreatedAt
                };
            }
            catch (Exception ex)
            {
                return new CreateWaterLevelResponse
                {
                    Success = false,
                    Message = $"Error recording water level: {ex.Message}"
                };
            }
        }
    }
}






