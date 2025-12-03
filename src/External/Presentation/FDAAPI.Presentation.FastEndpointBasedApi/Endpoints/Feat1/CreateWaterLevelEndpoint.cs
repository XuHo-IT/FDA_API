using FDAAPI.App.Common.Features;
using FDAAPI.App.Feat1;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1.DTOs;
using FastEndpoints;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1
{
    /// <summary>
    /// Endpoint for creating a water level record
    /// 
    /// Request Flow:
    ///   1. Client sends POST request to /api/v1/water-levels
    ///   2. FastEndpoint deserializes request DTO
    ///   3. Endpoint extracts data and maps to request
    ///   4. Creates CreateWaterLevelRequest and sends to handler
    ///   5. Handler executes business logic
    ///   6. Endpoint maps response and sends back to client
    /// </summary>
    public class CreateWaterLevelEndpoint : Endpoint<CreateWaterLevelRequestDto, CreateWaterLevelResponseDto>
    {
        private readonly IFeatureHandler<CreateWaterLevelRequest, CreateWaterLevelResponse> _handler;

        public CreateWaterLevelEndpoint(IFeatureHandler<CreateWaterLevelRequest, CreateWaterLevelResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            // Define HTTP method and route
            Post("/api/v1/water-levels");

            // Authentication requirement - uncomment when JWT is implemented
            // RequireAuthorization();

            // Allow anonymous access for demo purposes
            AllowAnonymous();

            // API documentation/summary
            Summary(s =>
            {
                s.Summary = "Create a new water level record";
                s.Description = "Records a new water level measurement for a specific location.";
                s.ExampleRequest = new CreateWaterLevelRequestDto 
                { 
                    LocationName = "River Main",
                    WaterLevel = 2.5,
                    Unit = "meters"
                };
            });

            Tags("Water Levels", "Create");
        }

        public override async Task HandleAsync(CreateWaterLevelRequestDto req, CancellationToken ct)
        {
            try
            {
                // Step 1: Create application request from DTO
                var appRequest = new CreateWaterLevelRequest
                {
                    LocationName = req.LocationName,
                    WaterLevel = req.WaterLevel,
                    Unit = req.Unit,
                    MeasuredAt = req.MeasuredAt
                };

                // Step 2: Execute handler (business logic layer)
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Step 3: Map result to response DTO and send
                var responseDto = new CreateWaterLevelResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    WaterLevelId = result.WaterLevelId,
                    LocationName = result.LocationName,
                    WaterLevel = result.WaterLevel,
                    CreatedAt = result.CreatedAt
                };

                if (result.Success)
                {
                    // Send 201 Created on successful creation
                    await SendAsync(responseDto, 201, ct);
                }
                else
                {
                    // Send 400 Bad Request if validation failed
                    await SendAsync(responseDto, 400, ct);
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                var errorDto = new CreateWaterLevelResponseDto { Success = false, Message = $"An unexpected error occurred: {ex.Message}" };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}
