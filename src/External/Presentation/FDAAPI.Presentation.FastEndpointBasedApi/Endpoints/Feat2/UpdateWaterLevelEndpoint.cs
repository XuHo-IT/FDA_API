using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Feat2;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2
{
    /// <summary>
    /// Endpoint for updating a water level record
    /// 
    /// Request Flow:
    /// 	1. Client sends PUT request to /api/v1/water-levels/{waterLevelId}
    /// 	2. FastEndpoint binds {waterLevelId} from route and JSON body into req (UpdateWaterLevelRequestDto).
    /// 	3. Endpoint uses req.WaterLevelId and req.NewWaterLevel.
    /// 	4. Creates UpdateWaterLevelRequest and sends to handler
    /// 	5. Handler executes business logic
    /// 	6. Endpoint maps response and sends back to client
    /// </summary>
    public class UpdateWaterLevelEndpoint : Endpoint<UpdateWaterLevelRequestDto, UpdateWaterLevelResponseDto>
    {
        private readonly IFeatureHandler<UpdateWaterLevelRequest, UpdateWaterLevelResponse> _handler;

        public UpdateWaterLevelEndpoint(IFeatureHandler<UpdateWaterLevelRequest, UpdateWaterLevelResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            // Define HTTP method and route
            Put("/api/v1/water-levels/{waterLevelId}");

            // Allow anonymous access for demo purposes
            AllowAnonymous();

            // API documentation/summary
            Summary(s =>
            {
                s.Summary = "Update an existing water level record";
                s.Description = "Updates the water level measurement for a specific record.";
                s.ExampleRequest = new UpdateWaterLevelRequestDto
                {
                    WaterLevelId = 123, // Thêm ID vào ví dụ để rõ ràng
                    NewWaterLevel = 3.5,
                    LocationName = "River A",
                    Unit = "meters",
                    UpdatedAt = DateTime.UtcNow
                };
            });

            Tags("Water Levels", "Update");
        }

        public override async Task HandleAsync(UpdateWaterLevelRequestDto req, CancellationToken ct)
        {
            try
            {
                // KHÔNG CẦN Route<string>("waterLevelId") nữa. 
                // req.WaterLevelId đã được FastEndpoints bind tự động từ route.

                // Step 1: Create application request
                var appRequest = new UpdateWaterLevelRequest
                {
                    WaterLevelId = req.WaterLevelId, // Lấy ID đã được bind
                    NewWaterLevel = req.NewWaterLevel,
                    LocationName = req.LocationName,
                    Unit = req.Unit,
                    UpdatedAt = req.UpdatedAt
                };

                // Step 2: Execute handler (business logic layer)
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Step 3: Map result to response DTO and send
                var responseDto = new UpdateWaterLevelResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    WaterLevelId = result.WaterLevelId,
                    NewWaterLevel = result.NewWaterLevel,
                    LocationName = result.LocationName,
                    UpdatedAt = result.UpdatedAt
                };

                if (result.Success)
                {
                    // Send 200 OK on successful update
                    await SendAsync(responseDto, 200, ct);
                }
                else
                {
                    // Send 404 Not Found hoặc 400 Bad Request tùy thuộc vào logic của handler
                    // Ví dụ: 404 nếu ID không tồn tại, 400 nếu validation thất bại
                    await SendAsync(responseDto, 400, ct);
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                var errorDto = new UpdateWaterLevelResponseDto { Success = false, Message = $"An unexpected error occurred: {ex.Message}" };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}