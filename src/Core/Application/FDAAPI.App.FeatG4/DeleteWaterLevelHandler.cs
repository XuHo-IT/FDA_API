using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.Feat4
{
    /// <summary>
    /// Handler for deleting a water level record
    /// Implements the business logic for removing water level measurements
    /// </summary>
    public class DeleteWaterLevelHandler : IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse>
    {
        private readonly IWaterLevelRepository _waterLevelRepository;

        public DeleteWaterLevelHandler(IWaterLevelRepository waterLevelRepository)
        {
            _waterLevelRepository = waterLevelRepository;
        }

        public async Task<DeleteWaterLevelResponse> ExecuteAsync(DeleteWaterLevelRequest request, CancellationToken ct)
        {
            // Validation: Check if request is valid
            if (request.WaterLevelId <= 0)
            {
                return new DeleteWaterLevelResponse
                {
                    Success = false,
                    Message = "Invalid water level ID"
                };
            }

            try
            {
                var deleted = await _waterLevelRepository.DeleteAsync(request.WaterLevelId);

                if (!deleted)
                {
                    return new DeleteWaterLevelResponse
                    {
                        Success = false,
                        Message = $"Water level with ID {request.WaterLevelId} not found"
                    };
                }

                return new DeleteWaterLevelResponse
                {
                    Success = true,
                    Message = "Water level deleted successfully",
                    WaterLevelId = request.WaterLevelId,
                    DeletedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new DeleteWaterLevelResponse
                {
                    Success = false,
                    Message = $"Error deleting water level: {ex.Message}"
                };
            }
        }
    }
}
