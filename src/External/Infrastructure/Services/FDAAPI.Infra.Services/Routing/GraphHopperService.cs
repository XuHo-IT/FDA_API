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
            var ghRequest = new
            {
                points = request.Points,
                profile = request.Profile,
                points_encoded = request.PointsEncoded,
                instructions = request.Instructions,
                alternative_route = request.AlternativeRoute != null ? new
                {
                    max_paths = request.AlternativeRoute.MaxPaths,
                    max_weight_factor = request.AlternativeRoute.MaxWeightFactor
                } : null,
                avoid_polygons = BuildAvoidPolygons(request.AvoidPolygons)
            };

            return ghRequest;
        }

        /// <summary>
        /// GraphHopper expects avoid_polygons as a single GeoJSON geometry object.
        /// Multiple polygons → MultiPolygon with coordinates: [ [ring1], [ring2], ... ]
        /// Single polygon → Polygon with coordinates: [ [ring] ]
        /// </summary>
        private object? BuildAvoidPolygons(List<GeoJsonGeometry>? polygons)
        {
            if (polygons == null || !polygons.Any())
                return null;

            var rings = polygons
                .Select(p => FlatToRing(p.Coordinates))
                .ToList();

            if (rings.Count == 1)
            {
                return new
                {
                    type = "Polygon",
                    coordinates = new[] { rings[0] }
                };
            }

            return new
            {
                type = "MultiPolygon",
                coordinates = rings.Select(r => new[] { r }).ToArray()
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
