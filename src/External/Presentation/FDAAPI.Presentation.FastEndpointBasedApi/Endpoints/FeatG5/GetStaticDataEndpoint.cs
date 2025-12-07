using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG5;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG5.DTOs;

public class GetStaticDataEndpoint
    : EndpointWithoutRequest<GetStaticDataResponseDto>

{
    private readonly IFeatureHandler<GetStaticDataRequest, GetStaticDataResponse> _handler;

    public GetStaticDataEndpoint(
        IFeatureHandler<GetStaticDataRequest, GetStaticDataResponse> handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/api/v1/static-data");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Get all static data";
            s.Description = "Retrieves DANANG_CENTER, MOCK_SENSORS, and FLOOD_ZONES data.";
        });

        Tags("Static Data", "Read");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var req = new GetStaticDataRequest();

        var result = await _handler.ExecuteAsync(req, ct);

        var response = new GetStaticDataResponseDto
        {
            Success = result.Success,
            Message = result.Message,
            DanangCenter = result.DanangCenter,
            MockSensors = result.MockSensors,
            FloodZones = result.FloodZones
        };

        await SendAsync(response, result.Success ? 200 : 500, ct);
    }
}
