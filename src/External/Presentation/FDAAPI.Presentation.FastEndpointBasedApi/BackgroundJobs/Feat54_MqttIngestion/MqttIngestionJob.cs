using FDAAPI.App.FeatG1_SensorReadingCreate;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Text.Json;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat54_MqttIngestion
{
    /// <summary>
    /// Background service to listen to MQTT broker and save sensor readings to database
    /// </summary>
    public class MqttIngestionJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MqttIngestionJob> _logger;
        private readonly IConfiguration _configuration;
        private readonly IManagedMqttClient _mqttClient;

        // System User ID for IoT data ingestion
        private static readonly Guid SYSTEM_USER_ID = new Guid("00000000-0000-0000-0000-000000000001");

        public MqttIngestionJob(
            IServiceProvider serviceProvider,
            ILogger<MqttIngestionJob> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _mqttClient = new MqttFactory().CreateManagedMqttClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Read MQTT settings from appsettings.json
            var server = _configuration["MqttSettings:Server"];
            var port = int.Parse(_configuration["MqttSettings:Port"] ?? "1883");
            var username = _configuration["MqttSettings:User"];
            var password = _configuration["MqttSettings:Pass"];
            var topic = _configuration["MqttSettings:Topic"];

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(topic))
            {
                _logger.LogError("MQTT settings are not configured properly in appsettings.json");
                return;
            }

            _logger.LogInformation("Starting MQTT Ingestion Job...");
            _logger.LogInformation("MQTT Server: {Server}:{Port}", server, port);
            _logger.LogInformation("MQTT Topic: {Topic}", topic);

            // Configure MQTT client options
            var clientOptions = new MqttClientOptionsBuilder()
                .WithClientId("FDA_API_" + Guid.NewGuid().ToString())
                .WithTcpServer(server, port)
                .WithCredentials(username, password)
                .WithCleanSession()
                .Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(clientOptions)
                .Build();

            // Set up message handler
            _mqttClient.ApplicationMessageReceivedAsync += HandleMqttMessageAsync;

            // Start MQTT client
            await _mqttClient.StartAsync(managedOptions);

            // Subscribe to topic
            await _mqttClient.SubscribeAsync(new[]
            {
                new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .Build()
            });

            _logger.LogInformation("MQTT Ingestion Job started successfully. Listening to topic: {Topic}", topic);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            _logger.LogInformation("MQTT Ingestion Job is stopping...");
            await _mqttClient.StopAsync();
        }

        private async Task HandleMqttMessageAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            try
            {
                var payload = args.ApplicationMessage.ConvertPayloadToString();
                var topic = args.ApplicationMessage.Topic;

                _logger.LogInformation("Received MQTT message from topic: {Topic}", topic);
                _logger.LogDebug("Payload: {Payload}", payload);

                // Parse JSON payload
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var sensorData = JsonSerializer.Deserialize<MqttSensorDataDto>(payload, options);

                if (sensorData == null)
                {
                    _logger.LogWarning("Failed to deserialize MQTT payload: {Payload}", payload);
                    return;
                }

                // Validate station_id
                if (!Guid.TryParse(sensorData.StationId, out Guid stationId))
                {
                    _logger.LogWarning("Invalid station_id in MQTT payload: {StationId}", sensorData.StationId);
                    return;
                }

                // Always use server UTC time (ignore ESP32 timestamp)
                DateTime measuredAt = DateTime.UtcNow;

                // Create scope for scoped services (DbContext, Repositories, MediatR)
                using (var scope = _serviceProvider.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    // Create request to save sensor reading
                    var request = new CreateSensorReadingRequest(
                        CreatedByUserId: SYSTEM_USER_ID,
                        StationId: stationId,
                        Value: sensorData.WaterLevel,
                        Distance: sensorData.Distance,
                        SensorHeight: sensorData.SensorHeight,
                        Unit: "cm",
                        Status: sensorData.Status,
                        MeasuredAt: measuredAt
                    );

                    // Send request via MediatR (will save to database)
                    var result = await mediator.Send(request);

                    if (result.Success)
                    {
                        _logger.LogInformation(
                            "Successfully saved sensor reading. Station: {StationId}, Water Level: {WaterLevel}cm, Status: {Status}",
                            stationId,
                            sensorData.WaterLevel,
                            sensorData.Status == 1 ? "WARNING" : "NORMAL"
                        );
                    }
                    else
                    {
                        _logger.LogError(
                            "Failed to save sensor reading. Station: {StationId}, Error: {Message}",
                            stationId,
                            result.Message
                        );
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error in MQTT message handler");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MQTT Ingestion Job is stopping...");

            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.StopAsync();
                _mqttClient?.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}