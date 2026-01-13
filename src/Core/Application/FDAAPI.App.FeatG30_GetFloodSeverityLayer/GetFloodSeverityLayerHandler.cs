using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Map;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer
{
    public class GetFloodSeverityLayerHandler : IRequestHandler<GetFloodSeverityLayerRequest, GetFloodSeverityLayerResponse>
    {
        private readonly IStationRepository _stationRepository;

        public GetFloodSeverityLayerHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<GetFloodSeverityLayerResponse> Handle(
            GetFloodSeverityLayerRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get all stations with pagination (limit to 1000 for performance)
                var (stations, totalCount) = await _stationRepository.GetStationsAsync(
                    searchTerm: null,
                    status: "active",
                    pageNumber: 1,
                    pageSize: 1000,
                    ct);

                var stationsList = stations.ToList();

                // 2. Filter by bounds if provided
                if (request.Bounds != null)
                {
                    stationsList = stationsList.Where(s =>
                        s.Latitude.HasValue &&
                        s.Longitude.HasValue &&
                        s.Latitude.Value >= request.Bounds.MinLat &&
                        s.Latitude.Value <= request.Bounds.MaxLat &&
                        s.Longitude.Value >= request.Bounds.MinLng &&
                        s.Longitude.Value <= request.Bounds.MaxLng
                    ).ToList();
                }

                // 3. Build GeoJSON features
                var features = stationsList
                    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
                    .Select(station =>
                    {
                        // For now, use mock water level data
                        // TODO: Integrate with WaterLevel repository when available
                        var mockWaterLevel = GetMockWaterLevel(station.Id);
                        var (severity, level) = CalculateSeverity(mockWaterLevel);

                        return new GeoJsonFeature
                        {
                            Type = "Feature",
                            Geometry = new GeoJsonGeometry
                            {
                                Type = "Point",
                                Coordinates = new[] { station.Longitude!.Value, station.Latitude!.Value }
                            },
                            Properties = new
                            {
                                stationId = station.Id,
                                stationCode = station.Code,
                                stationName = station.Name,
                                waterLevel = mockWaterLevel,
                                unit = "meters",
                                severity = severity,
                                severityLevel = level,
                                lastUpdated = station.LastSeenAt ?? DateTime.UtcNow,
                                status = station.Status
                            }
                        };
                    })
                    .ToList();

                return new GetFloodSeverityLayerResponse
                {
                    Success = true,
                    Message = $"Retrieved {features.Count} stations",
                    StatusCode = GetFloodSeverityLayerResponseStatusCode.Success,
                    Data = new GeoJsonFeatureCollection
                    {
                        Type = "FeatureCollection",
                        Features = features,
                        Metadata = new
                        {
                            totalStations = features.Count,
                            generatedAt = DateTime.UtcNow,
                            bounds = request.Bounds
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetFloodSeverityLayerResponse
                {
                    Success = false,
                    Message = $"Error retrieving flood severity data: {ex.Message}",
                    StatusCode = GetFloodSeverityLayerResponseStatusCode.UnknownError
                };
            }
        }

        private (string severity, int level) CalculateSeverity(decimal waterLevel)
        {
            // Severity thresholds (can be made configurable later)
            if (waterLevel >= 3.0m)
                return ("critical", 3);  // Red

            if (waterLevel >= 2.0m)
                return ("warning", 2);   // Orange

            if (waterLevel >= 1.0m)
                return ("caution", 1);   // Yellow

            return ("safe", 0);          // Green
        }

        // TODO: Replace with actual WaterLevel repository query
        private decimal GetMockWaterLevel(Guid stationId)
        {
            // Mock data for demonstration
            var random = new Random(stationId.GetHashCode());
            return (decimal)(random.NextDouble() * 4); // 0-4 meters
        }
    }
}
