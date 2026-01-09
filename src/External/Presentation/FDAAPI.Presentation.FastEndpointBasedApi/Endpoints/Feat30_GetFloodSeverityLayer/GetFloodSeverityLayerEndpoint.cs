using FastEndpoints;
using FDAAPI.App.FeatG30_GetFloodSeverityLayer;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat30_GetFloodSeverityLayer
{
    public class GetFloodSeverityLayerEndpoint : EndpointWithoutRequest<object>
    {
        private readonly IMediator _mediator;

        public GetFloodSeverityLayerEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/map/flood-severity");
            AllowAnonymous(); // Public endpoint

            Summary(s =>
            {
                s.Summary = "Get flood severity layer data";
                s.Description = "Returns GeoJSON FeatureCollection with station locations and flood severity";
            });

            Tags("Map", "Flood");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                // Parse query parameters
                BoundingBox? bounds = null;
                var boundsStr = Query<string>("bounds", false);

                if (!string.IsNullOrEmpty(boundsStr))
                {
                    var parts = boundsStr.Split(',');
                    if (parts.Length == 4 &&
                        decimal.TryParse(parts[0], out var minLat) &&
                        decimal.TryParse(parts[1], out var minLng) &&
                        decimal.TryParse(parts[2], out var maxLat) &&
                        decimal.TryParse(parts[3], out var maxLng))
                    {
                        bounds = new BoundingBox
                        {
                            MinLat = minLat,
                            MinLng = minLng,
                            MaxLat = maxLat,
                            MaxLng = maxLng
                        };
                    }
                }

                var zoom = Query<int?>("zoom", false);

                var request = new GetFloodSeverityLayerRequest(bounds, zoom);
                var result = await _mediator.Send(request, ct);

                if (result.Success)
                {
                    // Return raw GeoJSON (not wrapped in response envelope)
                    await SendAsync(result.Data!, 200, ct);
                }
                else
                {
                    await SendAsync(new { error = result.Message }, 500, ct);
                }
            }
            catch (Exception ex)
            {
                await SendAsync(new { error = $"An error occurred: {ex.Message}" }, 500, ct);
            }
        }
    }
}
