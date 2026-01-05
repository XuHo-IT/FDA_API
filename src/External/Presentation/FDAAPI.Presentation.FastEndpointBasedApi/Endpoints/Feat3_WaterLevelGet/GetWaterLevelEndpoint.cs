using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG3_WaterLevelGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3_WaterLevelGet.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3_WaterLevelGet;

/// <summary>
/// Endpoint for retrieving a water level record
///
/// Request Flow:
/// 	1. Client sends GET request to /api/v1/water-levels/{waterLevelId}
/// 	2. FastEndpoint automatically binds {waterLevelId} from route to Request.WaterLevelId.
/// 	3. Endpoint creates GetWaterLevelRequest and sends to handler
/// 	4. Handler executes business logic to fetch data
/// 	5. Endpoint maps response and sends back to client
/// </summary>
public class GetWaterLevelEndpoint
: Endpoint<GetWaterLevelRequestDto, GetWaterLevelResponseDto> // S? d?ng Request DTO d? kích ho?t binding và hi?n th? trong Swagger/Scalar
{
    private readonly IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse> _handler;

    public GetWaterLevelEndpoint(
        IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse> handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        // FastEndpoints s? t? d?ng c? g?ng bind các thu?c tính c?a Request DTO
        // v?i các tham s? t? Route, Query, Header, ho?c Body.
        Get("/api/v1/water-levels/{waterLevelId}");

        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get a water level record";
            s.Description = "Retrieves a specific water level measurement by ID.";
        });

        Tags("Water Levels", "Read");
    }

    // Ðã s?a l?i: Phuong th?c HandleAsync(TRequest req, CancellationToken ct) là b?t bu?c
    // khi k? th?a t? Endpoint<TRequest, TResponse>.
    public override async Task HandleAsync(GetWaterLevelRequestDto req, CancellationToken ct)
    {
        // L?y ID dã du?c bind t? d?ng t? Route vào d?i s? 'req'
        var appRequest = new GetWaterLevelRequest
        {
            WaterLevelId = req.WaterLevelId
        };

        var result = await _handler.ExecuteAsync(appRequest, ct);

        var response = new GetWaterLevelResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            WaterLevelId = result.WaterLevelId,
            LocationName = result.LocationName,
            WaterLevel = result.WaterLevel,
            Unit = result.Unit,
            MeasuredAt = result.MeasuredAt
        };

        // S? d?ng mã tr?ng thái 404 n?u không tìm th?y, 200 n?u thành công
        await SendAsync(response, result.Success ? 200 : 404, ct);
    }
}








