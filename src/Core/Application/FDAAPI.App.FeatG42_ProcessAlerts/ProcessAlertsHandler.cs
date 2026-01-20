using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Infra.Services.Alerts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG42_ProcessAlerts
{
    public class ProcessAlertsHandler : IRequestHandler<ProcessAlertsRequest, ProcessAlertsResponse>
    {
        private readonly IAlertRuleRepository _ruleRepo;
        private readonly ISensorReadingRepository _sensorRepo;
        private readonly IAlertRepository _alertRepo;
        private readonly IAlertCooldownConfigRepository _cooldownRepo;
        private readonly IStationRepository _stationRepo;
        private readonly IGlobalThresholdService _globalThresholdService;
        private readonly ILogger<ProcessAlertsHandler> _logger;

        public ProcessAlertsHandler(
            IAlertRuleRepository ruleRepo,
            ISensorReadingRepository sensorRepo,
            IAlertRepository alertRepo,
            IAlertCooldownConfigRepository cooldownRepo,
            IStationRepository stationRepo,
            IGlobalThresholdService globalThresholdService,
            ILogger<ProcessAlertsHandler> logger)
        {
            _ruleRepo = ruleRepo;
            _sensorRepo = sensorRepo;
            _alertRepo = alertRepo;
            _cooldownRepo = cooldownRepo;
            _stationRepo = stationRepo;
            _globalThresholdService = globalThresholdService;
            _logger = logger;
        }

        public async Task<ProcessAlertsResponse> Handle(ProcessAlertsRequest request, CancellationToken ct)
        {
            int created = 0, updated = 0, skippedCooldown = 0;

            try
            {
                // ===== HYBRID APPROACH: Process ALL stations, not just those with rules =====
                // 1. Get all active stations
                var allStations = await _stationRepo.GetAllAsync(ct);
                var activeStations = allStations.Where(s => s.Status == "active").ToList();
                var stationIds = activeStations.Select(s => s.Id).ToArray();

                _logger.LogInformation("Processing alerts for {StationCount} active stations", stationIds.Length);

                // 2. Get latest sensor readings for all active stations
                var latestReadings = await _sensorRepo.GetLatestReadingsByStationsAsync(stationIds, ct);

                foreach (var reading in latestReadings)
                {
                    // 3. Check if custom AlertRule exists for this station
                    var allRules = await _ruleRepo.GetActiveRulesByStationAsync(reading.StationId, ct);
                    var customRules = allRules.Where(r => !r.IsGlobalDefault).ToList();

                    if (customRules.Any())
                    {
                        // Use custom AlertRule logic (override global)
                        _logger.LogDebug(
                            "Station {StationId}: Using {Count} custom rules",
                            reading.StationId, customRules.Count);

                        foreach (var rule in customRules)
                        {
                            var result = await ProcessCustomRule(rule, reading, ct);
                            created += result.created;
                            updated += result.updated;
                            skippedCooldown += result.skipped;
                        }
                    }
                    else
                    {
                        // Use Global Threshold logic
                        _logger.LogDebug(
                            "Station {StationId}: Using global thresholds (no custom rules)",
                            reading.StationId);

                        var result = await ProcessGlobalThreshold(reading, ct);
                        created += result.created;
                        updated += result.updated;
                        skippedCooldown += result.skipped;
                    }
                }

                // 4. Get pending alerts count
                var pendingAlerts = await _alertRepo.GetUnnotifiedAlertsAsync(1000, ct);

                _logger.LogInformation(
                    "Alert processing completed. Created: {Created}, Updated: {Updated}, " +
                    "Skipped (cooldown): {Skipped}, Pending notifications: {Pending}",
                    created, updated, skippedCooldown, pendingAlerts.Count());

                return new ProcessAlertsResponse
                {
                    Success = true,
                    Message = $"Processed {latestReadings.Count()} sensor readings",
                    AlertsCreated = created,
                    AlertsUpdated = updated,
                    AlertsPending = pendingAlerts.Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing alerts");
                return new ProcessAlertsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Process alert using custom AlertRule (admin-defined threshold)
        /// </summary>
        private async Task<(int created, int updated, int skipped)> ProcessCustomRule(
            AlertRule rule,
            SensorReading reading,
            CancellationToken ct)
        {
            int created = 0, updated = 0, skipped = 0;

            // Check if threshold exceeded
            bool thresholdExceeded = rule.RuleType.ToLower() switch
            {
                "threshold" => (decimal)reading.Value >= rule.ThresholdValue,
                _ => false
            };

            if (!thresholdExceeded)
            {
                _logger.LogDebug(
                    "Custom rule {RuleId}: Threshold not exceeded. Value: {Value}, Threshold: {Threshold}",
                    rule.Id, reading.Value, rule.ThresholdValue);
                return (0, 0, 0);
            }

            // Check if alert already exists
            var existingAlerts = await _alertRepo.GetActiveAlertsByStationAsync(rule.StationId, ct);
            var existingAlert = existingAlerts
                .FirstOrDefault(a => a.AlertRuleId == rule.Id && a.Status == "open");

            if (existingAlert != null)
            {
                var result = await HandleExistingAlert(existingAlert, reading, ct);
                return result == "updated" ? (0, 1, 0) : (0, 0, 1);
            }
            else
            {
                await CreateNewAlert(rule.Id, rule.StationId, rule.Severity, reading, ct);
                return (1, 0, 0);
            }
        }

        /// <summary>
        /// Process alert using global threshold (0-10 safe, 10-20 caution, 20-40 warning, 40+ critical)
        /// </summary>
        private async Task<(int created, int updated, int skipped)> ProcessGlobalThreshold(
            SensorReading reading,
            CancellationToken ct)
        {
            var (severity, level) = _globalThresholdService.CalculateSeverity((decimal)reading.Value);

            // Only create alert if severity >= caution (level 1+)
            if (level == 0) // safe
            {
                _logger.LogDebug(
                    "Station {StationId}: Water level {Value}cm is safe, no alert needed",
                    reading.StationId, reading.Value);
                return (0, 0, 0);
            }

            // Check if alert already exists for this severity
            var existingAlerts = await _alertRepo.GetActiveAlertsByStationAsync(reading.StationId, ct);
            var existingAlert = existingAlerts
                .FirstOrDefault(a => a.Severity == severity && a.Status == "open" && a.AlertRuleId == null);

            if (existingAlert != null)
            {
                var result = await HandleExistingAlert(existingAlert, reading, ct);
                return result == "updated" ? (0, 1, 0) : (0, 0, 1);
            }
            else
            {
                await CreateNewAlert(null, reading.StationId, severity, reading, ct);
                return (1, 0, 0);
            }
        }

        /// <summary>
        /// Handle existing alert with cooldown logic
        /// </summary>
        private async Task<string> HandleExistingAlert(
            Alert existingAlert,
            SensorReading reading,
            CancellationToken ct)
        {
            // ===== COOLDOWN LOGIC ===== ✅
            var cooldownMinutes = await _cooldownRepo.GetCooldownMinutesAsync(existingAlert.Severity, ct);

            var timeSinceLastNotification = existingAlert.LastNotificationAt.HasValue
                ? DateTime.UtcNow - existingAlert.LastNotificationAt.Value
                : (TimeSpan?)null;

            bool cooldownActive = timeSinceLastNotification.HasValue &&
                                 timeSinceLastNotification.Value.TotalMinutes < cooldownMinutes;

            if (cooldownActive)
            {
                // Within cooldown period - just update value, don't trigger notification
                _logger.LogDebug(
                    "Alert {AlertId} within cooldown period ({Minutes}min). " +
                    "Last notification: {LastNotif}, Cooldown: {Cooldown}min",
                    existingAlert.Id,
                    timeSinceLastNotification.Value.TotalMinutes,
                    existingAlert.LastNotificationAt,
                    cooldownMinutes);

                existingAlert.CurrentValue = (decimal)reading.Value;
                existingAlert.UpdatedAt = DateTime.UtcNow;
                existingAlert.UpdatedBy = Guid.Empty; // System user

                await _alertRepo.UpdateAsync(existingAlert, ct);
                return "skipped";
            }

            // Cooldown expired - allow new notification
            _logger.LogInformation(
                "Cooldown expired for Alert {AlertId}. Allowing new notification. " +
                "Minutes since last: {Minutes}, Cooldown period: {Cooldown}",
                existingAlert.Id,
                timeSinceLastNotification?.TotalMinutes ?? 0,
                cooldownMinutes);

            existingAlert.CurrentValue = (decimal)reading.Value;
            existingAlert.NotificationSent = false; // Reset to trigger new notification
            existingAlert.UpdatedAt = DateTime.UtcNow;
            existingAlert.UpdatedBy = Guid.Empty;

            await _alertRepo.UpdateAsync(existingAlert, ct);
            return "updated";
        }

        /// <summary>
        /// Create new alert
        /// </summary>
        private async Task CreateNewAlert(
            Guid? alertRuleId,
            Guid stationId,
            string severity,
            SensorReading reading,
            CancellationToken ct)
        {
            var systemUserId = Guid.Empty;

            var station = await _stationRepo.GetByIdAsync(stationId, ct);
            var stationName = station?.Name ?? "Unknown Station";

            var newAlert = new Alert
            {
                Id = Guid.NewGuid(),
                AlertRuleId = alertRuleId, // Null for global threshold alerts
                StationId = stationId,
                TriggeredAt = DateTime.UtcNow,
                Status = "open",
                Severity = severity,
                Priority = MapSeverityToPriority(severity),
                CurrentValue = (decimal)reading.Value,
                Message = alertRuleId.HasValue
                    ? $"Water level {reading.Value}{reading.Unit} exceeds custom threshold at {stationName}"
                    : $"Water level {reading.Value}{reading.Unit} reaches {severity} level (global threshold) at {stationName}",
                NotificationSent = false,
                NotificationCount = 0,
                LastNotificationAt = null,
                CreatedBy = systemUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = systemUserId,
                UpdatedAt = DateTime.UtcNow
            };

            await _alertRepo.CreateAsync(newAlert, ct);

            _logger.LogInformation(
                "Created new alert {AlertId} for station {StationId}. Severity: {Severity}, Value: {Value}, Type: {Type}",
                newAlert.Id, stationId, severity, reading.Value,
                alertRuleId.HasValue ? "Custom Rule" : "Global Threshold");
        }

        private NotificationPriority MapSeverityToPriority(string severity)
        {
            return severity.ToLower() switch
            {
                "critical" => NotificationPriority.Critical,
                "warning" => NotificationPriority.High,
                "caution" => NotificationPriority.Medium,
                "info" => NotificationPriority.Low,
                _ => NotificationPriority.Low
            };
        }
    }
}