namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat54_MqttIngestion.Services
{
    /// <summary>
    /// Service for broadcasting real-time map updates via SignalR
    /// </summary>
    public interface IRealtimeMapService
    {
        Task BroadcastSensorUpdateAsync(
            Guid stationId,
            double waterLevel,
            double distance,
            double sensorHeight,
            int status,
            DateTime measuredAt,
            CancellationToken ct = default
        );
    }
}