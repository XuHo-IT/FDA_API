using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.App.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Routing
{
    public class GraphHopperService : IGraphHopperService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GraphHopperService> _logger;

        public GraphHopperService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GraphHopperService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["GraphHopper:BaseUrl"] ?? "http://localhost:8989";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.Timeout = TimeSpan.FromMilliseconds(
                _configuration.GetValue<int>("GraphHopper:Timeout", 30000));
        }

        public async Task<GraphHopperRouteResponse> GetRouteAsync(
            GraphHopperRouteRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var requestBody = BuildGraphHopperRequest(request);
                var response = await _httpClient.PostAsJsonAsync("/route", requestBody, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GraphHopperRouteResponse>(ct);
                return result ?? throw new InvalidOperationException("Empty response from GraphHopper");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "GraphHopper API request failed");
                throw new InvalidOperationException("Routing service unavailable", ex);
            }
        }

        private object BuildGraphHopperRequest(GraphHopperRouteRequest request)
        {
            var hasAvoidPolygons = request.AvoidPolygons != null && request.AvoidPolygons.Any();

            var ghRequest = new Dictionary<string, object?>
            {
                ["points"] = request.Points,
                ["profile"] = request.Profile,
                ["points_encoded"] = request.PointsEncoded,
                ["instructions"] = request.Instructions
            };

            if (request.AlternativeRoute != null)
            {
                ghRequest["alternative_route"] = new
                {
                    max_paths = request.AlternativeRoute.MaxPaths,
                    max_weight_factor = request.AlternativeRoute.MaxWeightFactor
                };
            }

            // GraphHopper uses custom_model with areas to avoid polygons.
            // Requires ch.disable=true (flexible/hybrid mode).
            if (hasAvoidPolygons)
            {
                ghRequest["ch.disable"] = true;
                ghRequest["custom_model"] = BuildCustomModel(request.AvoidPolygons!);
            }

            return ghRequest;
        }

        /// <summary>
        /// Build GraphHopper custom_model with areas for flood avoidance.
        /// Format: { "priority": [{ "if": "in_flood_zone", "multiply_by": 0 }],
        ///           "areas": { "flood_zone": { "type": "Feature", "geometry": {...} } } }
        /// </summary>
        private object BuildCustomModel(List<GeoJsonGeometry> polygons)
        {
            var rings = polygons
                .Select(p => FlatToRing(p.Coordinates))
                .ToArray();

            // Combine all flood polygons into a single MultiPolygon
            var geometry = new
            {
                type = "MultiPolygon",
                coordinates = rings.Select(r => new[] { r }).ToArray()
            };

            return new
            {
                priority = new[]
                {
                    new { @if = "in_flood_zone", multiply_by = 0.0 }
                },
                areas = new Dictionary<string, object>
                {
                    ["flood_zone"] = new
                    {
                        type = "Feature",
                        geometry,
                        properties = new { }
                    }
                }
            };
        }

        private decimal[][] FlatToRing(decimal[] coords)
        {
            var points = new List<decimal[]>();
            for (int i = 0; i < coords.Length; i += 2)
            {
                points.Add(new[] { coords[i], coords[i + 1] });
            }
            return points.ToArray();
        }
    }

}
