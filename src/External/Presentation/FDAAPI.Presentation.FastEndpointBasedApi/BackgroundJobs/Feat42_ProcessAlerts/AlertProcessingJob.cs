using FDAAPI.App.FeatG42_ProcessAlerts;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat42_ProcessAlerts
{
    /// <summary>
    /// Background job to periodically check sensor readings and create alerts
    /// Runs every X minutes (configurable)
    /// </summary>
    public class AlertProcessingJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AlertProcessingJob> _logger;
        private readonly TimeSpan _interval;

        public AlertProcessingJob(
            IServiceProvider serviceProvider,
            ILogger<AlertProcessingJob> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Read interval from appsettings.json, default to 2 minutes
            var intervalMinutes = configuration.GetValue<int>("BackgroundJobs:AlertProcessing:IntervalMinutes", 2);
            _interval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Alert Processing Job started. Interval: {Interval}", _interval);

            // Wait a bit before first execution to let the app start up
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Alert Processing Job: Starting alert processing cycle");

                    // Create a scope to resolve scoped services (like DbContext, Repositories)
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        var request = new ProcessAlertsRequest();
                        var result = await mediator.Send(request, stoppingToken);

                        if (result.Success)
                        {
                            _logger.LogInformation(
                                "Alert Processing Job completed successfully. " +
                                "Created: {Created}, Updated: {Updated}, Pending: {Pending}",
                                result.AlertsCreated,
                                result.AlertsUpdated,
                                result.AlertsPending);
                        }
                        else
                        {
                            _logger.LogWarning("Alert Processing Job failed: {Message}", result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Alert Processing Job");
                }

                // Wait for the next cycle
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Alert Processing Job stopped");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Alert Processing Job is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }
}