using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG2_WaterLevelUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2_WaterLevelUpdate.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2_WaterLevelUpdate{
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
                    WaterLevelId = 123, // Thêm ID vào ví d? d? rõ ràng
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
                // KHÔNG C?N Route<string>("waterLevelId") n?a. 
                // req.WaterLevelId dã du?c FastEndpoints bind t? d?ng t? route.

                // Step 1: Create application request
                var appRequest = new UpdateWaterLevelRequest
                {
                    WaterLevelId = req.WaterLevelId, // L?y ID dã du?c bind
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
                    // Send 404 Not Found ho?c 400 Bad Request tùy thu?c vào logic c?a handler
                    // Ví d?: 404 n?u ID không t?n t?i, 400 n?u validation th?t b?i
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








