using FDAAPI.App.FeatG43_DispatchNotifications;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat43_DispatchNotifications
{
    /// <summary>
    /// Background job to dispatch pending notifications
    /// Runs more frequently than AlertProcessingJob (every 30s-1min)
    /// </summary>
    public class NotificationDispatchJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationDispatchJob> _logger;
        private readonly TimeSpan _interval;

        public NotificationDispatchJob(
            IServiceProvider serviceProvider,
            ILogger<NotificationDispatchJob> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Default to 1 minute
            var intervalMinutes = configuration.GetValue<int>("BackgroundJobs:NotificationDispatch:IntervalMinutes", 1);
            _interval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Dispatch Job started. Interval: {Interval}", _interval);

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Notification Dispatch Job: Starting dispatch cycle");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        var request = new DispatchNotificationsRequest();
                        var result = await mediator.Send(request, stoppingToken);

                        if (result.Success)
                        {
                            _logger.LogInformation(
                                "Notification Dispatch Job completed. " +
                                "Alerts: {Alerts}, Created: {Created}, Sent: {Sent}, Failed: {Failed}",
                                result.AlertsProcessed,
                                result.NotificationsCreated,
                                result.NotificationsSent,
                                result.NotificationsFailed);
                        }
                        else
                        {
                            _logger.LogWarning("Notification Dispatch Job failed: {Message}", result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Notification Dispatch Job");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Notification Dispatch Job stopped");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Dispatch Job is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}