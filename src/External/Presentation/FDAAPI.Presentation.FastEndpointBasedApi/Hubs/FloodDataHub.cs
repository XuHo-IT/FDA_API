using Microsoft.AspNetCore.SignalR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time flood data updates
    /// </summary>
    public class FloodDataHub : Hub
    {
        private readonly ILogger<FloodDataHub> _logger;

        public FloodDataHub(ILogger<FloodDataHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToStation(string stationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"station_{stationId}");
            _logger.LogInformation("Client {ConnectionId} subscribed to station {StationId}",
                Context.ConnectionId, stationId);
        }

        public async Task UnsubscribeFromStation(string stationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"station_{stationId}");
        }
    }
}