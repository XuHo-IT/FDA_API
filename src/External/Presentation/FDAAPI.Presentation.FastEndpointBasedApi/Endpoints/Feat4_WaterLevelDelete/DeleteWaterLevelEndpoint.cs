using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG4_WaterLevelDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4_WaterLevelDelete.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4_WaterLevelDelete{
    /// <summary>
    /// Endpoint for deleting a water level record by ID.
    /// </summary>
    public class DeleteWaterLevelEndpoint : Endpoint<DeleteWaterLevelRequestDto, DeleteWaterLevelResponseDto> // S?A: S? d?ng DTO Request d? binding ID
    {
        private readonly IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse> _handler;

        public DeleteWaterLevelEndpoint(IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            // Ð?nh nghia HTTP method và route
            Delete("/api/v1/water-levels/{waterLevelId}");

            // Cho phép truy c?p ?n danh (AllowAnonymous)
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
                // KHÔNG C?N Route<string>("waterLevelId") n?a. 
                // req.WaterLevelId dã du?c FastEndpoints bind t? d?ng t? route.

                // Bu?c 1: T?o application request
                var appRequest = new DeleteWaterLevelRequest
                {
                    WaterLevelId = req.WaterLevelId // L?y ID dã du?c bind
                };

                // Bu?c 2: Execute handler (Business Logic Layer)
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Bu?c 3: Map result to response DTO and send
                var responseDto = new DeleteWaterLevelResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    WaterLevelId = result.WaterLevelId,
                    DeletedAt = result.DeletedAt
                };

                if (result.Success)
                {
                    // S?A L?I: Tr? v? 200 OK cho DELETE thành công khi có body
                    await SendAsync(responseDto, 200, ct);
                }
                else
                {
                    // Tr? v? 404 Not Found n?u b?n ghi không t?n t?i
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








