using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Feat3;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3;

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
: Endpoint<GetWaterLevelRequestDto, GetWaterLevelResponseDto> // Sử dụng Request DTO để kích hoạt binding và hiển thị trong Swagger/Scalar
{
    private readonly IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse> _handler;

    public GetWaterLevelEndpoint(
        IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse> handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        // FastEndpoints sẽ tự động cố gắng bind các thuộc tính của Request DTO
        // với các tham số từ Route, Query, Header, hoặc Body.
        Get("/api/v1/water-levels/{waterLevelId}");

        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get a water level record";
            s.Description = "Retrieves a specific water level measurement by ID.";
        });

        Tags("Water Levels", "Read");
    }

    // Đã sửa lỗi: Phương thức HandleAsync(TRequest req, CancellationToken ct) là bắt buộc
    // khi kế thừa từ Endpoint<TRequest, TResponse>.
    public override async Task HandleAsync(GetWaterLevelRequestDto req, CancellationToken ct)
    {
        // Lấy ID đã được bind tự động từ Route vào đối số 'req'
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

        // Sử dụng mã trạng thái 404 nếu không tìm thấy, 200 nếu thành công
        await SendAsync(response, result.Success ? 200 : 404, ct);
    }
}