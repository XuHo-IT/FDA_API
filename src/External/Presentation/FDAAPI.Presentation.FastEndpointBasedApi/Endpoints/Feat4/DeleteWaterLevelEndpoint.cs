using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Feat4;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4
{
    /// <summary>
    /// Endpoint for deleting a water level record by ID.
    /// </summary>
    public class DeleteWaterLevelEndpoint : Endpoint<DeleteWaterLevelRequestDto, DeleteWaterLevelResponseDto> // SỬA: Sử dụng DTO Request để binding ID
    {
        private readonly IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse> _handler;

        public DeleteWaterLevelEndpoint(IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            // Định nghĩa HTTP method và route
            Delete("/api/v1/water-levels/{waterLevelId}");

            // Cho phép truy cập ẩn danh (AllowAnonymous)
            AllowAnonymous();

            // API documentation/summary
            Summary(s =>
            {
                s.Summary = "Delete a water level record by ID";
                s.Description = "Removes a specific water level measurement using the ID provided in the route.";
            });

            Tags("Water Levels", "Delete");
        }

        public override async Task HandleAsync(DeleteWaterLevelRequestDto req, CancellationToken ct)
        {
            try
            {
                // KHÔNG CẦN Route<string>("waterLevelId") nữa. 
                // req.WaterLevelId đã được FastEndpoints bind tự động từ route.

                // Bước 1: Tạo application request
                var appRequest = new DeleteWaterLevelRequest
                {
                    WaterLevelId = req.WaterLevelId // Lấy ID đã được bind
                };

                // Bước 2: Execute handler (Business Logic Layer)
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Bước 3: Map result to response DTO and send
                var responseDto = new DeleteWaterLevelResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    WaterLevelId = result.WaterLevelId,
                    DeletedAt = result.DeletedAt
                };

                if (result.Success)
                {
                    // SỬA LỖI: Trả về 200 OK cho DELETE thành công khi có body
                    await SendAsync(responseDto, 200, ct);
                }
                else
                {
                    // Trả về 404 Not Found nếu bản ghi không tồn tại
                    await SendAsync(responseDto, 404, ct);
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                var errorDto = new DeleteWaterLevelResponseDto { Success = false, Message = $"An unexpected error occurred: {ex.Message}" };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}