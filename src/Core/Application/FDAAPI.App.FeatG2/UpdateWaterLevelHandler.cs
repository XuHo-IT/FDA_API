using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.Feat2
{
    /// <summary>
    /// Handler for updating a water level record
    /// Implements the business logic for updating water level measurements
    /// </summary>
    public class UpdateWaterLevelHandler : IFeatureHandler<UpdateWaterLevelRequest, UpdateWaterLevelResponse>
    {
        private readonly IWaterLevelRepository _waterLevelRepository;

        public UpdateWaterLevelHandler(IWaterLevelRepository waterLevelRepository)
        {
            _waterLevelRepository = waterLevelRepository;
        }

        public async Task<UpdateWaterLevelResponse> ExecuteAsync(UpdateWaterLevelRequest request, CancellationToken ct)
        {
            if (request.WaterLevelId <= 0)
            {
                return new UpdateWaterLevelResponse
                {
                    Success = false,
                    Message = "Invalid water level ID"
                };
            }

            if (request.NewWaterLevel < 0)
            {
                return new UpdateWaterLevelResponse
                {
                    Success = false,
                    Message = "Water level cannot be negative"
                };
            }

            try
            {
                var existingWaterLevel = await _waterLevelRepository.GetByIdAsync(request.WaterLevelId);

                if (existingWaterLevel == null)
                {
                    return new UpdateWaterLevelResponse
                    {
                        Success = false,
                        Message = $"Water level with ID {request.WaterLevelId} not found"
                    };
                }

                existingWaterLevel.Value = request.NewWaterLevel;
                existingWaterLevel.Unit = request.Unit;
                existingWaterLevel.UpdatedAt = (DateTime?)DateTime.UtcNow;

                await _waterLevelRepository.UpdateAsync(existingWaterLevel);

                return new UpdateWaterLevelResponse
                {
                    Success = true,
                    Message = "Water level updated successfully",
                    WaterLevelId = request.WaterLevelId,
                    NewWaterLevel = request.NewWaterLevel,
                    UpdatedAt = existingWaterLevel.UpdatedAt.Value
                };
            }
            catch (Exception ex)
            {
                return new UpdateWaterLevelResponse
                {
                    Success = false,
                    Message = $"Error updating water level: {ex.Message}"
                };
            }
        }
    }
}
