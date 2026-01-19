using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Presentation.FastEndpointBasedApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat54_MqttIngestion.Services
{
    public class RealtimeMapService : IRealtimeMapService
    {
        private readonly IHubContext<FloodDataHub> _hubContext;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<RealtimeMapService> _logger;

        public RealtimeMapService(
            IHubContext<FloodDataHub> hubContext,
            AppDbContext dbContext,
            ILogger<RealtimeMapService> logger)
        {
            _hubContext = hubContext;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task BroadcastSensorUpdateAsync(
            Guid stationId,
            double waterLevel,
            double distance,
            double sensorHeight,
            int status,
            DateTime measuredAt,
            CancellationToken ct = default)
        {
            try
            {
                var station = await _dbContext.Stations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == stationId, ct);

                if (station == null)
                {
                    _logger.LogWarning("Station not found: {StationId}", stationId);
                    return;
                }

                var (severity, severityLevel) = CalculateFloodSeverity(waterLevel);

                var update = new
                {
                    type = "sensor_update",
                    timestamp = DateTime.UtcNow,
                    data = new
                    {
                        stationId = stationId,
                        stationCode = station.Code,
                        stationName = station.Name,
                        latitude = station.Latitude,
                        longitude = station.Longitude,
                        waterLevel = waterLevel,
                        distance = distance,
                        sensorHeight = sensorHeight,
                        unit = "cm",
                        status = status,
                        severity = severity,
                        severityLevel = severityLevel,
                        markerColor = GetMarkerColor(severityLevel),
                        alertLevel = GetAlertLevel(severityLevel),
                        measuredAt = measuredAt
                    }
                };

                await _hubContext.Clients.All.SendAsync("ReceiveSensorUpdate", update, ct);
                await _hubContext.Clients.Group($"station_{stationId}")
                    .SendAsync("ReceiveStationUpdate", update, ct);

                _logger.LogDebug("Broadcasted update for station {Code}", station.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting sensor update");
            }
        }

        private (string severity, int level) CalculateFloodSeverity(double waterLevel)
        {
            double waterLevelInMeters = waterLevel / 100.0;
            if (waterLevelInMeters >= 0.4) return ("critical", 3);
            if (waterLevelInMeters >= 0.2) return ("warning", 2);
            if (waterLevelInMeters >= 0.1) return ("caution", 1);
            return ("safe", 0);
        }

        private string GetMarkerColor(int severityLevel) => severityLevel switch
        {
            3 => "#DC2626",
            2 => "#F97316",
            1 => "#FCD34D",
            0 => "#10B981",
            _ => "#9CA3AF"
        };

        private string GetAlertLevel(int severityLevel) => severityLevel switch
        {
            3 => "CRITICAL",
            2 => "WARNING",
            1 => "CAUTION",
            0 => "SAFE",
            _ => "NO DATA"
        };
    }
}